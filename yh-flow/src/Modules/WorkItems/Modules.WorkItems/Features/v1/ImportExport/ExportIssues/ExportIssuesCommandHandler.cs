using System.Text;
using System.Text.Json;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Export.ExportIssues;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.ImportExport.ExportIssues;

/// <summary>
/// Handles <see cref="ExportIssuesCommand"/> — exports issues as CSV or JSON.
/// CSV output includes formula injection protection (T-4-import-01).
/// </summary>
public sealed class ExportIssuesCommandHandler : ICommandHandler<ExportIssuesCommand, ExportResultDto>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private readonly WorkItemsDbContext _db;

    public ExportIssuesCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<ExportResultDto> Handle(ExportIssuesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ProjectId == Guid.Empty)
        {
            throw new CustomException("Project id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        // Query issues with filters
        IQueryable<Issue> query = _db.Issues
            .AsNoTracking()
            .Where(i => i.ProjectId == command.ProjectId && !i.IsDeleted && !i.IsDraft);

        if (command.StateIds is { Count: > 0 })
        {
            query = query.Where(i => i.StateId.HasValue && command.StateIds.Contains(i.StateId.Value));
        }

        if (!string.IsNullOrWhiteSpace(command.Priority))
        {
            query = query.Where(i => i.Priority == command.Priority);
        }

        var issues = await query
            .OrderBy(i => i.CreatedOnUtc)
            .Select(i => new ExportRow
            {
                Name = i.Name,
                DescriptionHtml = i.DescriptionHtml,
                Priority = i.Priority,
                StateId = i.StateId,
                SequenceId = i.SequenceId,
                CreatedOnUtc = i.CreatedOnUtc,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Fetch state names
        var states = await _db.States
            .AsNoTracking()
            .Where(s => s.ProjectId == command.ProjectId)
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken)
            .ConfigureAwait(false);

        var isJson = string.Equals(command.Format ?? "csv", "json", StringComparison.OrdinalIgnoreCase);

        if (isJson)
        {
            return ExportJson(issues, states);
        }

        return ExportCsv(issues, states);
    }

    private static ExportResultDto ExportCsv(
        IReadOnlyList<ExportRow> issues,
        Dictionary<Guid, string> states)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("Name,DescriptionHtml,Priority,StateName,SequenceId,CreatedAt");

        foreach (var issue in issues)
        {
            var stateName = issue.StateId is not null && states.TryGetValue(issue.StateId.Value, out var sn)
                ? sn
                : string.Empty;

            sb.AppendLine(string.Join(",",
                CsvEscape(issue.Name),
                CsvEscape(issue.DescriptionHtml ?? string.Empty),
                CsvEscape(issue.Priority),
                CsvEscape(stateName),
                issue.SequenceId.ToString(),
                issue.CreatedOnUtc.ToString("O")));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());

        return new ExportResultDto
        {
            FileContent = bytes,
            ContentType = "text/csv",
            FileName = $"issues-export-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv",
        };
    }

    private static ExportResultDto ExportJson(
        IReadOnlyList<ExportRow> issues,
        Dictionary<Guid, string> states)
    {
        var rows = issues.Select(i => new
        {
            name = i.Name,
            description_html = i.DescriptionHtml,
            priority = i.Priority,
            state_name = i.StateId is not null && states.TryGetValue(i.StateId.Value, out var sn) ? sn : null,
            sequence_id = i.SequenceId,
            created_at = i.CreatedOnUtc.ToString("O"),
        }).ToList();

        var json = JsonSerializer.Serialize(rows, JsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        return new ExportResultDto
        {
            FileContent = bytes,
            ContentType = "application/json",
            FileName = $"issues-export-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.json",
        };
    }

    /// <summary>
    /// Escapes a CSV field value with formula injection protection (T-4-import-01).
    /// Prefixes cells starting with =, +, -, @ with a single quote.
    /// Wraps in quotes if the value contains commas, newlines, or quotes.
    /// </summary>
    private static string CsvEscape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Formula injection protection: prefix dangerous starters with '
        if (value.Length > 0 && (value[0] == '=' || value[0] == '+' || value[0] == '-' || value[0] == '@'))
        {
            value = "'" + value;
        }

        // Escape quotes by doubling them, wrap in quotes if needed
        if (value.Contains('"', StringComparison.Ordinal) || value.Contains(',', StringComparison.Ordinal)
            || value.Contains('\n', StringComparison.Ordinal) || value.Contains('\r', StringComparison.Ordinal))
        {
            value = "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
        }

        return value;
    }

    /// <summary>
    /// Internal projection for export rows.
    /// </summary>
    private sealed class ExportRow
    {
        public string Name { get; set; } = default!;
        public string DescriptionHtml { get; set; } = default!;
        public string Priority { get; set; } = default!;
        public Guid? StateId { get; set; }
        public int SequenceId { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; }
    }
}
