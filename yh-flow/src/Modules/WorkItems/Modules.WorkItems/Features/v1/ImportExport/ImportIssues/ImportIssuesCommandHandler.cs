using System.Globalization;
using System.Text;
using System.Text.Json;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Import.ImportIssues;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;
using YH.Modules.WorkItems.Services;

namespace YH.Modules.WorkItems.Features.v1.ImportExport.ImportIssues;

/// <summary>
/// Handles <see cref="ImportIssuesCommand"/> — imports issues from CSV or JSON.
/// Per-row validation with error reporting (T-4-import-02, T-4-import-03).
/// </summary>
public sealed class ImportIssuesCommandHandler : ICommandHandler<ImportIssuesCommand, ImportResultDto>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private readonly WorkItemsDbContext _db;
    private readonly IIssueSequenceService _sequenceService;

    public ImportIssuesCommandHandler(WorkItemsDbContext db, IIssueSequenceService sequenceService)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _sequenceService = sequenceService ?? throw new ArgumentNullException(nameof(sequenceService));
    }

    public async ValueTask<ImportResultDto> Handle(ImportIssuesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ProjectId == Guid.Empty)
        {
            throw new CustomException("Project id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        if (command.FileContent is null || command.FileContent.Length == 0)
        {
            throw new CustomException("File content is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var isJson = string.Equals(command.Format ?? "csv", "json", StringComparison.OrdinalIgnoreCase);

        List<IssueImportRowDto> rows;
        if (isJson)
        {
            rows = ParseJson(command.FileContent);
        }
        else
        {
            rows = ParseCsv(command.FileContent);
        }

        if (rows.Count == 0)
        {
            throw new CustomException("No valid rows found in import file.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var result = new ImportResultDto();

        // Resolve default state for the project
        var defaultState = await _db.States
            .Where(s => s.ProjectId == command.ProjectId && s.IsDefault)
            .OrderBy(s => s.SortOrder)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        // Pre-fetch existing state names for mapping
        var existingStates = await _db.States
            .Where(s => s.ProjectId == command.ProjectId)
            .ToDictionaryAsync(s => s.Name, s => s.Id, cancellationToken)
            .ConfigureAwait(false);

        // Process each row in a transaction
        using var transaction = await _db.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            foreach (var row in rows)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(row.Name))
                    {
                        result.Skipped++;
                        result.Errors.Add("Skipped row: Name is required.");
                        continue;
                    }

                    var priority = ValidatePriority(row.Priority);
                    if (priority is null)
                    {
                        result.Skipped++;
                        result.Errors.Add($"Skipped row '{row.Name}': Invalid priority '{row.Priority}'.");
                        continue;
                    }

                    // Resolve state
                    Guid? stateId = null;
                    if (!string.IsNullOrWhiteSpace(row.StateName))
                    {
                        if (existingStates.TryGetValue(row.StateName, out var sid))
                        {
                            stateId = sid;
                        }
                        else
                        {
                            // Create new state with same name in Unstarted group
                            var newState = State.Create(
                                name: row.StateName,
                                color: "#60646C",
                                group: StateGroup.Unstarted,
                                projectId: command.ProjectId,
                                isDefault: false);

                            _db.States.Add(newState);
                            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                            stateId = newState.Id;
                            existingStates[row.StateName] = newState.Id;
                        }
                    }
                    else
                    {
                        stateId = defaultState?.Id;
                    }

                    // Create the issue
                    var issue = Issue.Create(
                        name: row.Name,
                        projectId: command.ProjectId,
                        stateId: stateId,
                        descriptionHtml: row.DescriptionHtml,
                        priority: priority);

                    var sequenceId = await _sequenceService.GetNextSequenceIdAsync(command.ProjectId, cancellationToken).ConfigureAwait(false);
                    issue.SequenceId = sequenceId;

                    _db.Issues.Add(issue);
                    await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    result.Imported++;
                }
                catch (Exception ex) when (ex is not CustomException)
                {
                    result.Skipped++;
                    result.Errors.Add($"Error processing row '{row.Name ?? "(unnamed)"}': {ex.Message}");
                }
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }

        return result;
    }

    private static List<IssueImportRowDto> ParseJson(byte[] content)
    {
        var json = Encoding.UTF8.GetString(content);
        var rows = JsonSerializer.Deserialize<List<IssueImportRowDto>>(json, JsonOptions);
        return rows ?? [];
    }

    private static List<IssueImportRowDto> ParseCsv(byte[] content)
    {
        var text = Encoding.UTF8.GetString(content);
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 2)
            return [];

        // Parse header
        var header = ParseCsvLine(lines[0].Trim('\r'));
        var nameIdx = header.FindIndex(h => h.Equals("name", StringComparison.OrdinalIgnoreCase));
        var descIdx = header.FindIndex(h => h.Equals("descriptionhtml", StringComparison.OrdinalIgnoreCase)
                                          || h.Equals("description_html", StringComparison.OrdinalIgnoreCase));
        var priorityIdx = header.FindIndex(h => h.Equals("priority", StringComparison.OrdinalIgnoreCase));
        var stateNameIdx = header.FindIndex(h => h.Equals("statename", StringComparison.OrdinalIgnoreCase)
                                                || h.Equals("state_name", StringComparison.OrdinalIgnoreCase));

        if (nameIdx < 0)
            return []; // Name column is required

        var rows = new List<IssueImportRowDto>();

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim('\r');
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = ParseCsvLine(line);

            var row = new IssueImportRowDto
            {
                Name = nameIdx < fields.Count ? fields[nameIdx] : string.Empty,
                DescriptionHtml = descIdx >= 0 && descIdx < fields.Count ? fields[descIdx] : null,
                Priority = priorityIdx >= 0 && priorityIdx < fields.Count ? fields[priorityIdx] : null,
                StateName = stateNameIdx >= 0 && stateNameIdx < fields.Count ? fields[stateNameIdx] : null,
            };

            rows.Add(row);
        }

        return rows;
    }

    /// <summary>
    /// Parses a single CSV line into fields, handling quoted fields.
    /// </summary>
    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;
        var i = 0;

        while (i < line.Length)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i += 2;
                    continue;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }

            i++;
        }

        fields.Add(current.ToString());
        return fields;
    }

    /// <summary>
    /// Validates and normalizes priority string.
    /// </summary>
    private static string? ValidatePriority(string? priority)
    {
        if (string.IsNullOrWhiteSpace(priority))
            return "none";

        return priority.ToUpperInvariant() switch
        {
            "URGENT" => "urgent",
            "HIGH" => "high",
            "MEDIUM" => "medium",
            "LOW" => "low",
            "NONE" => "none",
            _ => null,
        };
    }
}
