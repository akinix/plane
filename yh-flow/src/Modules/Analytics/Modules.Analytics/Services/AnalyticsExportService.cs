using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YH.Modules.Analytics.Contracts.DTOs;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.Analytics.Services;

/// <summary>
/// Generates CSV exports of Issue data for analytics.
/// Uses <see cref="IServiceScopeFactory"/> to resolve scoped DbContext for Hangfire jobs.
/// </summary>
public sealed class AnalyticsExportService : IAnalyticsExportService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AnalyticsExportService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    }

    /// <inheritdoc />
    public string ContentType => "text/csv";

    /// <inheritdoc />
    public async Task<byte[]> GenerateCsvAsync(string tenantId, string slug, string? projectIds, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WorkItemsDbContext>();

        var query = from issue in dbContext.Issues
                    join state in dbContext.States
                            .Where(s => !s.IsDeleted && s.DeletedOnUtc == null)
                        on issue.StateId equals state.Id into stateJoin
                    from state in stateJoin.DefaultIfEmpty()
                    where issue.TenantId == tenantId
                       && issue.DeletedOnUtc == null
                    select new
                    {
                        issue.Id,
                        issue.Name,
                        StateGroup = state != null ? state.Group.ToString() : "none",
                        issue.Priority,
                        issue.ProjectId,
                        issue.CreatedOnUtc,
                        issue.CompletedAt,
                    };

        if (!string.IsNullOrEmpty(projectIds))
        {
            var pIds = projectIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(Guid.Parse).ToList();
            query = query.Where(x => pIds.Contains(x.ProjectId));
        }

        var issues = await query.ToListAsync(ct);

        // Fetch assignees in bulk (avoid N+1)
        var issueIds = issues.Select(i => i.Id).ToList();
        var assigneeLookup = await dbContext.IssueAssignees
            .Where(a => a.TenantId == tenantId && issueIds.Contains(a.IssueId))
            .GroupBy(a => a.IssueId)
            .Select(g => new { IssueId = g.Key, Assignees = g.Select(a => a.AssigneeId).ToList() })
            .ToListAsync(ct);

        var assigneeMap = assigneeLookup.ToDictionary(a => a.IssueId, a => a.Assignees);

        var sb = new StringBuilder();

        // Header row
        sb.AppendLine("Issue ID,Name,State Group,Priority,Project Name,Assignee,Created At,Completed At");

        // Data rows
        foreach (var issue in issues)
        {
            var assignees = assigneeMap.TryGetValue(issue.Id, out var list)
                ? string.Join("; ", list)
                : string.Empty;

            sb.AppendLine(string.Join(",",
                CsvEscape(issue.Id.ToString()),
                CsvEscape(issue.Name),
                CsvEscape(issue.StateGroup ?? "none"),
                CsvEscape(issue.Priority),
                CsvEscape(issue.ProjectId.ToString()),
                CsvEscape(assignees),
                CsvEscape(issue.CreatedOnUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
                CsvEscape(issue.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? string.Empty)
            ));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    /// <summary>
    /// Escapes a value for CSV: wraps in quotes if contains comma, quote, or newline.
    /// Doubles embedded quotes per RFC 4180.
    /// </summary>
    private static string CsvEscape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(',', StringComparison.Ordinal) || value.Contains('"', StringComparison.Ordinal)
            || value.Contains('\n', StringComparison.Ordinal) || value.Contains('\r', StringComparison.Ordinal))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }
}
