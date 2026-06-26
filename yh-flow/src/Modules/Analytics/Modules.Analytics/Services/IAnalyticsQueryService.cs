using YH.Modules.Analytics.Contracts.DTOs;

namespace YH.Modules.Analytics.Services;

/// <summary>
/// Analytics query service — encapsulates all real-time aggregation queries against
/// <c>WorkItemsDbContext</c>. No caching; every call performs live COUNT/GROUP BY.
/// </summary>
public interface IAnalyticsQueryService
{
    /// <summary>
    /// Workspace overview — returns total_work_items, total_cycles, total_modules counts.
    /// </summary>
    /// <param name="slug">Workspace slug (for context, not used directly in query).</param>
    /// <param name="dateFilter">Predefined filter: this_month|last_7_days|last_30_days|last_3_months|custom.</param>
    /// <param name="startDate">Custom start date (YYYY-MM-DD) when <paramref name="dateFilter"/> is "custom".</param>
    /// <param name="endDate">Custom end date (YYYY-MM-DD) when <paramref name="dateFilter"/> is "custom".</param>
    /// <param name="projectIds">Comma-separated project IDs to filter by.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<AnalyticsOverviewDto> GetWorkspaceOverviewAsync(
        string slug, string? dateFilter, string? startDate, string? endDate, string? projectIds, CancellationToken ct);

    /// <summary>
    /// Work item status distribution — grouped by state group (backlog/unstarted/started/completed/cancelled).
    /// </summary>
    Task<WorkItemStatsDto> GetWorkItemStatsAsync(
        string slug, string? dateFilter, string? startDate, string? endDate, string? projectIds, CancellationToken ct);

    /// <summary>
    /// Project-level work item status distribution — same as <see cref="GetWorkItemStatsAsync"/> but scoped to a single project.
    /// </summary>
    Task<WorkItemStatsDto> GetProjectWorkItemStatsAsync(
        string slug, Guid projectId, string? dateFilter, string? startDate, string? endDate, CancellationToken ct);

    /// <summary>
    /// Workspace stats — issue count grouped by project, with per-state-group breakdown.
    /// </summary>
    Task<List<ProjectStatsDto>> GetProjectGroupedStatsAsync(
        string slug, string? dateFilter, string? startDate, string? endDate, string? projectIds, CancellationToken ct);

    /// <summary>
    /// Project stats — issue count grouped by assignee, with per-state-group breakdown.
    /// </summary>
    Task<List<AssigneeStatsDto>> GetAssigneeGroupedStatsAsync(
        string slug, Guid projectId, string? dateFilter, string? startDate, string? endDate, CancellationToken ct);
}
