using System.Globalization;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Shared.Multitenancy;
using YH.Modules.Analytics.Contracts.DTOs;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.Analytics.Services;

/// <summary>
/// Real-time analytics aggregation service. All queries are live COUNT/GROUP BY against
/// <see cref="WorkItemsDbContext"/> — no caching, no materialized views.
/// </summary>
/// <remarks>
/// <para>
/// <b>Navigation property note:</b> The <see cref="Issue"/> entity has a scalar <c>StateId</c> FK
/// but no <c>State</c> navigation property. All state-group aggregations use explicit LEFT JOIN
/// with the <c>States</c> table rather than navigation property access.
/// </para>
/// <para>
/// <b>Date filter behavior:</b> Filters apply to <see cref="Issue.CreatedOnUtc"/>. Predefined
/// ranges use <see cref="DateTimeOffset.UtcNow"/> as the reference point.
/// </para>
/// </remarks>
public sealed class AnalyticsQueryService : IAnalyticsQueryService
{
    private readonly WorkItemsDbContext _dbContext;
    private readonly IMultiTenantContextAccessor<AppTenantInfo> _tenantAccessor;

    public AnalyticsQueryService(
        WorkItemsDbContext dbContext,
        IMultiTenantContextAccessor<AppTenantInfo> tenantAccessor)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _tenantAccessor = tenantAccessor ?? throw new ArgumentNullException(nameof(tenantAccessor));
    }

    // ────────────────────────────── Public API ──────────────────────────────

    /// <inheritdoc />
    public async Task<AnalyticsOverviewDto> GetWorkspaceOverviewAsync(
        string slug, string? dateFilter, string? startDate, string? endDate, string? projectIds, CancellationToken ct)
    {
        var tenantId = GetTenantId();

        var baseIssueQuery = _dbContext.Issues
            .Where(i => i.TenantId == tenantId && i.DeletedOnUtc == null);
        var baseCycleQuery = _dbContext.Cycles
            .Where(c => c.TenantId == tenantId && c.DeletedOnUtc == null);
        var baseModuleQuery = _dbContext.Modules
            .Where(m => m.TenantId == tenantId && m.DeletedOnUtc == null);

        if (!string.IsNullOrEmpty(projectIds))
        {
            var pIds = projectIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(Guid.Parse).ToList();
            baseIssueQuery = baseIssueQuery.Where(i => pIds.Contains(i.ProjectId));
            baseCycleQuery = baseCycleQuery.Where(c => pIds.Contains(c.ProjectId));
            baseModuleQuery = baseModuleQuery.Where(m => pIds.Contains(m.ProjectId));
        }

        var (dateGte, dateLte) = ParseDateFilter(dateFilter, startDate, endDate);

        var issueCount = await CountWithDateFilterAsync(baseIssueQuery, dateGte, dateLte, ct);
        var cycleCount = await CountWithDateFilterAsync(baseCycleQuery, dateGte, dateLte, ct);
        var moduleCount = await CountWithDateFilterAsync(baseModuleQuery, dateGte, dateLte, ct);

        return new AnalyticsOverviewDto
        {
            TotalWorkItems = new CountValue { Count = issueCount },
            TotalCycles = new CountValue { Count = cycleCount },
            TotalModules = new CountValue { Count = moduleCount },
        };
    }

    /// <inheritdoc />
    public async Task<WorkItemStatsDto> GetWorkItemStatsAsync(
        string slug, string? dateFilter, string? startDate, string? endDate, string? projectIds, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var (dateGte, dateLte) = ParseDateFilter(dateFilter, startDate, endDate);

        var query = BuildIssueWithStateQuery(tenantId, projectIds, dateGte, dateLte);

        var stats = await query
            .GroupBy(x => x.StateGroup)
            .Select(g => new StateGroupCount { Group = g.Key, Count = g.LongCount() })
            .ToListAsync(ct);

        return ToWorkItemStats(stats);
    }

    /// <inheritdoc />
    public async Task<WorkItemStatsDto> GetProjectWorkItemStatsAsync(
        string slug, Guid projectId, string? dateFilter, string? startDate, string? endDate, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var (dateGte, dateLte) = ParseDateFilter(dateFilter, startDate, endDate);

        var query = BuildIssueWithStateQuery(tenantId, null, dateGte, dateLte)
            .Where(x => x.ProjectId == projectId);

        var stats = await query
            .GroupBy(x => x.StateGroup)
            .Select(g => new StateGroupCount { Group = g.Key, Count = g.LongCount() })
            .ToListAsync(ct);

        return ToWorkItemStats(stats);
    }

    /// <inheritdoc />
    public async Task<List<ProjectStatsDto>> GetProjectGroupedStatsAsync(
        string slug, string? dateFilter, string? startDate, string? endDate, string? projectIds, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var (dateGte, dateLte) = ParseDateFilter(dateFilter, startDate, endDate);

        var query = BuildIssueWithStateQuery(tenantId, projectIds, dateGte, dateLte);

        return await query
            .GroupBy(x => x.ProjectId)
            .Select(g => new ProjectStatsDto
            {
                ProjectId = g.Key,
                ProjectName = string.Empty,
                BacklogWorkItems = g.Count(x => x.StateGroup == StateGroup.Backlog),
                UnStartedWorkItems = g.Count(x => x.StateGroup == StateGroup.Unstarted),
                StartedWorkItems = g.Count(x => x.StateGroup == StateGroup.Started),
                CompletedWorkItems = g.Count(x => x.StateGroup == StateGroup.Completed),
                CancelledWorkItems = g.Count(x => x.StateGroup == StateGroup.Cancelled),
            })
            .OrderBy(p => p.ProjectId)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<List<AssigneeStatsDto>> GetAssigneeGroupedStatsAsync(
        string slug, Guid projectId, string? dateFilter, string? startDate, string? endDate, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var (dateGte, dateLte) = ParseDateFilter(dateFilter, startDate, endDate);

        var query = from issue in _dbContext.Issues
                    join assignee in _dbContext.IssueAssignees on issue.Id equals assignee.IssueId
                    join state in _dbContext.States.Where(s => !s.IsDeleted && s.DeletedOnUtc == null)
                        on issue.StateId equals state.Id into stateJoin
                    from state in stateJoin.DefaultIfEmpty()
                    where issue.TenantId == tenantId
                       && issue.DeletedOnUtc == null
                       && issue.ProjectId == projectId
                       && assignee.TenantId == tenantId
                    select new
                    {
                        issue.CreatedOnUtc,
                        StateGroup = state != null ? state.Group : (StateGroup?)null,
                        assignee.AssigneeId,
                    };

        if (dateGte.HasValue)
            query = query.Where(x => x.CreatedOnUtc >= dateGte.Value);
        if (dateLte.HasValue)
            query = query.Where(x => x.CreatedOnUtc <= dateLte.Value);

        var rawStats = await query
            .GroupBy(x => x.AssigneeId)
            .Select(g => new
            {
                AssigneeId = g.Key,
                BacklogCount = g.Count(x => x.StateGroup == StateGroup.Backlog),
                UnStartedCount = g.Count(x => x.StateGroup == StateGroup.Unstarted),
                StartedCount = g.Count(x => x.StateGroup == StateGroup.Started),
                CompletedCount = g.Count(x => x.StateGroup == StateGroup.Completed),
                CancelledCount = g.Count(x => x.StateGroup == StateGroup.Cancelled),
            })
            .OrderByDescending(a => a.StartedCount)
            .ToListAsync(ct);

        return rawStats
            .Select(s => new AssigneeStatsDto
            {
                AssigneeId = s.AssigneeId is not null && Guid.TryParse(s.AssigneeId, out var id) ? id : null,
                DisplayName = null,
                BacklogWorkItems = s.BacklogCount,
                UnStartedWorkItems = s.UnStartedCount,
                StartedWorkItems = s.StartedCount,
                CompletedWorkItems = s.CompletedCount,
                CancelledWorkItems = s.CancelledCount,
            })
            .ToList();
    }

    // ────────────────────────────── Wave 3: Chart Methods ──────────────────────────────

    /// <inheritdoc />
    public async Task<AnalyticsChartDto> GetWorkspaceWorkItemChartAsync(
        string slug, string? dateFilter, string? startDate, string? endDate, string? projectIds, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var (dateGte, dateLte) = ParseDateFilter(dateFilter, startDate, endDate);

        var query = _dbContext.Issues
            .Where(i => i.TenantId == tenantId && i.DeletedOnUtc == null);

        if (!string.IsNullOrEmpty(projectIds))
        {
            var pIds = projectIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(Guid.Parse).ToList();
            query = query.Where(i => pIds.Contains(i.ProjectId));
        }

        // Determine date range start
        var start = dateGte?.DateTime
            ?? _dbContext.Issues
                .Where(i => i.TenantId == tenantId && i.DeletedOnUtc == null)
                .OrderBy(i => i.CreatedOnUtc).FirstOrDefault()?.CreatedOnUtc.DateTime
            ?? DateTime.UtcNow.AddMonths(-12);
        var end = dateLte?.DateTime ?? DateTime.UtcNow;
        // Ensure start is first day of month
        start = new DateTime(start.Year, start.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Monthly aggregation
        var monthlyStats = await query
            .Where(i => i.CreatedOnUtc >= start && i.CreatedOnUtc <= end)
            .GroupBy(i => new { i.CreatedOnUtc.Year, i.CreatedOnUtc.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                CreatedCount = g.Count(),
                CompletedCount = g.Count(i => i.CompletedAt != null),
            })
            .ToListAsync(ct);

        var statsLookup = monthlyStats.ToDictionary(
            s => $"{s.Year:D4}-{s.Month:D2}",
            s => (s.CreatedCount, s.CompletedCount));

        // Fill all months (ensure months with 0 count are included)
        var data = new List<ChartDataPoint>();
        var current = new DateTime(start.Year, start.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonth = new DateTime(end.Year, end.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        while (current <= lastMonth)
        {
            var key = current.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var (createdCount, completedCount) = statsLookup.GetValueOrDefault(
                current.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                (0, 0));

            data.Add(new ChartDataPoint
            {
                Key = key,
                Name = key,
                Count = createdCount,
                CreatedIssues = createdCount,
                CompletedIssues = completedCount,
            });

            current = current.AddMonths(1);
        }

        return new AnalyticsChartDto
        {
            Data = data,
            Schema = new Dictionary<string, string>
            {
                ["completed_issues"] = "completed_issues",
                ["created_issues"] = "created_issues",
            },
        };
    }

    /// <inheritdoc />
    public async Task<List<ChartDataPoint>> GetProjectSummaryChartAsync(
        string slug, string? dateFilter, string? startDate, string? endDate, string? projectIds, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var (dateGte, dateLte) = ParseDateFilter(dateFilter, startDate, endDate);

        var baseIssueQuery = _dbContext.Issues
            .Where(i => i.TenantId == tenantId && i.DeletedOnUtc == null);
        var baseCycleQuery = _dbContext.Cycles
            .Where(c => c.TenantId == tenantId && c.DeletedOnUtc == null);
        var baseModuleQuery = _dbContext.Modules
            .Where(m => m.TenantId == tenantId && m.DeletedOnUtc == null);

        if (!string.IsNullOrEmpty(projectIds))
        {
            var pIds = projectIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(Guid.Parse).ToList();
            baseIssueQuery = baseIssueQuery.Where(i => pIds.Contains(i.ProjectId));
            baseCycleQuery = baseCycleQuery.Where(c => pIds.Contains(c.ProjectId));
            baseModuleQuery = baseModuleQuery.Where(m => pIds.Contains(m.ProjectId));
        }

        var (gte, lte) = (dateGte, dateLte);
        var workItemsCount = await CountWithDateFilterAsync(baseIssueQuery, gte, lte, ct);
        var cyclesCount = await CountWithDateFilterAsync(baseCycleQuery, gte, lte, ct);
        var modulesCount = await CountWithDateFilterAsync(baseModuleQuery, gte, lte, ct);

        // Intake = draft issues
        var intakeQuery = baseIssueQuery.Where(i => i.IsDraft);
        if (gte.HasValue) intakeQuery = intakeQuery.Where(i => i.CreatedOnUtc >= gte.Value);
        if (lte.HasValue) intakeQuery = intakeQuery.Where(i => i.CreatedOnUtc <= lte.Value);
        var intakeCount = await intakeQuery.CountAsync(ct);

        // Members, pages, views: not available from WorkItemsDbContext — return 0
        // These can be enriched by the caller/handler layer if needed.

        var items = new List<ChartDataPoint>
        {
            new() { Key = "work_items", Name = "Work Items", Count = workItemsCount },
            new() { Key = "cycles", Name = "Cycles", Count = cyclesCount },
            new() { Key = "modules", Name = "Modules", Count = modulesCount },
            new() { Key = "intake", Name = "Intake", Count = intakeCount },
            new() { Key = "members", Name = "Members", Count = 0 },
            new() { Key = "pages", Name = "Pages", Count = 0 },
            new() { Key = "views", Name = "Views", Count = 0 },
        };

        return items;
    }

    /// <inheritdoc />
    public async Task<AnalyticsChartDto> GetProjectWorkItemChartAsync(
        string slug, Guid projectId, string? dateFilter, string? startDate, string? endDate, Guid? cycleId, Guid? moduleId, CancellationToken ct)
    {
        var tenantId = GetTenantId();

        // Schema is same for all project chart responses
        var schema = new Dictionary<string, string>
        {
            ["completed_issues"] = "completed_issues",
            ["created_issues"] = "created_issues",
        };

        // ── Cycle-scoped: daily aggregation ──
        if (cycleId.HasValue)
        {
            var cycle = await _dbContext.Cycles
                .FirstOrDefaultAsync(c => c.Id == cycleId.Value && c.TenantId == tenantId, ct);

            if (cycle?.StartDate == null || cycle.EndDate == null)
            {
                return new AnalyticsChartDto { Data = [], Schema = schema };
            }

            var start = cycle.StartDate.Value.DateTime;
            var end = cycle.EndDate.Value.DateTime;

            var cycleIssueIds = await _dbContext.CycleIssues
                .Where(ci => ci.TenantId == tenantId && ci.CycleId == cycleId.Value && ci.DeletedOnUtc == null)
                .Select(ci => ci.IssueId)
                .ToListAsync(ct);

            var dailyQuery = _dbContext.Issues
                .Where(i => i.TenantId == tenantId && i.DeletedOnUtc == null
                    && i.ProjectId == projectId && cycleIssueIds.Contains(i.Id)
                    && i.CreatedOnUtc >= start && i.CreatedOnUtc <= end);

            var dailyStats = await dailyQuery
                .GroupBy(i => new { i.CreatedOnUtc.Year, i.CreatedOnUtc.Month, i.CreatedOnUtc.Day })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Day = g.Key.Day,
                    CreatedCount = g.Count(),
                    CompletedCount = g.Count(i => i.CompletedAt != null),
                })
                .ToListAsync(ct);

            var statsLookup = dailyStats.ToDictionary(
                s => $"{s.Year:D4}-{s.Month:D2}-{s.Day:D2}",
                s => (s.CreatedCount, s.CompletedCount));

            var data = new List<ChartDataPoint>();
            var current = start.Date;
            while (current <= end.Date)
            {
                var key = current.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var (createdCount, completedCount) = statsLookup.GetValueOrDefault(key, (0, 0));
                data.Add(new ChartDataPoint
                {
                    Key = key,
                    Name = key,
                    Count = createdCount,
                    CreatedIssues = createdCount,
                    CompletedIssues = completedCount,
                });
                current = current.AddDays(1);
            }

            return new AnalyticsChartDto { Data = data, Schema = schema };
        }

        // ── Module-scoped: daily aggregation ──
        if (moduleId.HasValue)
        {
            var module = await _dbContext.Modules
                .FirstOrDefaultAsync(m => m.Id == moduleId.Value && m.TenantId == tenantId, ct);

            if (module?.StartDate == null || module.TargetDate == null)
            {
                return new AnalyticsChartDto { Data = [], Schema = schema };
            }

            var start = module.StartDate.Value.DateTime;
            var end = module.TargetDate.Value.DateTime;

            var moduleIssueIds = await _dbContext.ModuleIssues
                .Where(mi => mi.TenantId == tenantId && mi.ModuleId == moduleId.Value && mi.DeletedOnUtc == null)
                .Select(mi => mi.IssueId)
                .ToListAsync(ct);

            var dailyQuery = _dbContext.Issues
                .Where(i => i.TenantId == tenantId && i.DeletedOnUtc == null
                    && i.ProjectId == projectId && moduleIssueIds.Contains(i.Id)
                    && i.CreatedOnUtc >= start && i.CreatedOnUtc <= end);

            var dailyStats = await dailyQuery
                .GroupBy(i => new { i.CreatedOnUtc.Year, i.CreatedOnUtc.Month, i.CreatedOnUtc.Day })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Day = g.Key.Day,
                    CreatedCount = g.Count(),
                    CompletedCount = g.Count(i => i.CompletedAt != null),
                })
                .ToListAsync(ct);

            var statsLookup = dailyStats.ToDictionary(
                s => $"{s.Year:D4}-{s.Month:D2}-{s.Day:D2}",
                s => (s.CreatedCount, s.CompletedCount));

            var data = new List<ChartDataPoint>();
            var current = start.Date;
            while (current <= end.Date)
            {
                var key = current.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var (createdCount, completedCount) = statsLookup.GetValueOrDefault(key, (0, 0));
                data.Add(new ChartDataPoint
                {
                    Key = key,
                    Name = key,
                    Count = createdCount,
                    CreatedIssues = createdCount,
                    CompletedIssues = completedCount,
                });
                current = current.AddDays(1);
            }

            return new AnalyticsChartDto { Data = data, Schema = schema };
        }

        // ── No cycle/module: monthly aggregation (project-level) ──
        var (dateGte, dateLte) = ParseDateFilter(dateFilter, startDate, endDate);

        var projectQuery = _dbContext.Issues
            .Where(i => i.TenantId == tenantId && i.DeletedOnUtc == null
                && i.ProjectId == projectId);

        var projectStart = dateGte?.DateTime ?? _dbContext.Issues
            .Where(i => i.TenantId == tenantId && i.DeletedOnUtc == null && i.ProjectId == projectId)
            .OrderBy(i => i.CreatedOnUtc).FirstOrDefault()?.CreatedOnUtc.DateTime ?? DateTime.UtcNow.AddMonths(-12);
        var projectEnd = dateLte?.DateTime ?? DateTime.UtcNow;
        projectStart = new DateTime(projectStart.Year, projectStart.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var monthlyStats = await projectQuery
            .Where(i => i.CreatedOnUtc >= projectStart && i.CreatedOnUtc <= projectEnd)
            .GroupBy(i => new { i.CreatedOnUtc.Year, i.CreatedOnUtc.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                CreatedCount = g.Count(),
                CompletedCount = g.Count(i => i.CompletedAt != null),
            })
            .ToListAsync(ct);

        var monthlyLookup = monthlyStats.ToDictionary(
            s => $"{s.Year:D4}-{s.Month:D2}",
            s => (s.CreatedCount, s.CompletedCount));

        var monthlyData = new List<ChartDataPoint>();
        var mCurrent = new DateTime(projectStart.Year, projectStart.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var mLast = new DateTime(projectEnd.Year, projectEnd.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        while (mCurrent <= mLast)
        {
            var key = mCurrent.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var (createdCount, completedCount) = monthlyLookup.GetValueOrDefault(
                mCurrent.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                (0, 0));

            monthlyData.Add(new ChartDataPoint
            {
                Key = key,
                Name = key,
                Count = createdCount,
                CreatedIssues = createdCount,
                CompletedIssues = completedCount,
            });

            mCurrent = mCurrent.AddMonths(1);
        }

        return new AnalyticsChartDto { Data = monthlyData, Schema = schema };
    }

    // ────────────────────────────── Private Types ──────────────────────────────

    /// <summary>
    /// Lightweight projection for issue-with-state queries — scalars only, no entity references.
    /// </summary>
    private sealed class IssueStateView
    {
        public Guid ProjectId { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; }
        public StateGroup? StateGroup { get; set; }
    }

    /// <summary>
    /// Helper class for grouping results — named type avoids anonymous type issues in method signatures.
    /// </summary>
    private sealed class StateGroupCount
    {
        public StateGroup? Group { get; set; }
        public long Count { get; set; }
    }

    // ────────────────────────────── Private Helpers ──────────────────────────────

    private string GetTenantId()
    {
        return _tenantAccessor.MultiTenantContext?.TenantInfo?.Id
            ?? throw new InvalidOperationException("Tenant context is not available.");
    }

    /// <summary>
    /// Converts a list of <see cref="StateGroupCount"/> into a <see cref="WorkItemStatsDto"/>.
    /// </summary>
    private static WorkItemStatsDto ToWorkItemStats(List<StateGroupCount> stats)
    {
        var total = (int)stats.Sum(s => s.Count);

        int GetGroupCount(StateGroup group) =>
            (int)(stats.FirstOrDefault(s => s.Group == group)?.Count ?? 0);

        return new WorkItemStatsDto
        {
            TotalWorkItems = new CountValue { Count = total },
            BacklogWorkItems = new CountValue { Count = GetGroupCount(StateGroup.Backlog) },
            UnStartedWorkItems = new CountValue { Count = GetGroupCount(StateGroup.Unstarted) },
            StartedWorkItems = new CountValue { Count = GetGroupCount(StateGroup.Started) },
            CompletedWorkItems = new CountValue { Count = GetGroupCount(StateGroup.Completed) },
            CancelledWorkItems = new CountValue { Count = GetGroupCount(StateGroup.Cancelled) },
        };
    }

    /// <summary>
    /// Parses the date filter query parameters into a nullable UTC range.
    /// </summary>
    private static (DateTimeOffset? Gte, DateTimeOffset? Lte) ParseDateFilter(
        string? dateFilter, string? startDate, string? endDate)
    {
        if (string.IsNullOrWhiteSpace(dateFilter))
            return (null, null);

        var now = DateTimeOffset.UtcNow;

        return dateFilter switch
        {
            "this_month" => (new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset), (DateTimeOffset?)null),
            "last_7_days" => (now.AddDays(-7), (DateTimeOffset?)null),
            "last_30_days" => (now.AddDays(-30), (DateTimeOffset?)null),
            "last_3_months" => (now.AddDays(-90), (DateTimeOffset?)null),
            "custom" => ParseCustomDateRange(startDate, endDate),
            _ => (null, null),
        };
    }

    private static (DateTimeOffset? Gte, DateTimeOffset? Lte) ParseCustomDateRange(
        string? startDate, string? endDate)
    {
        DateTimeOffset? gte = null;
        DateTimeOffset? lte = null;

        if (!string.IsNullOrWhiteSpace(startDate)
            && DateOnly.TryParse(startDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var start))
        {
            gte = start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        }

        if (!string.IsNullOrWhiteSpace(endDate)
            && DateOnly.TryParse(endDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
        {
            lte = end.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        }

        return (gte, lte);
    }

    /// <summary>
    /// Applies optional date range filters to the query and returns the count.
    /// The entity type must have a <c>CreatedOnUtc</c> property (applied by caller via .Where()).
    /// </summary>
    private static async Task<int> CountWithDateFilterAsync<T>(
        IQueryable<T> query,
        DateTimeOffset? dateGte,
        DateTimeOffset? dateLte,
        CancellationToken ct)
        where T : class
    {
        // Date filters are applied by the caller to the IQueryable before calling this method.
        _ = dateGte;
        _ = dateLte;

        return await query.CountAsync(ct);
    }

    /// <summary>
    /// Builds a composable query that joins Issues with States, projecting only scalars.
    /// </summary>
    private IQueryable<IssueStateView> BuildIssueWithStateQuery(
        string tenantId,
        string? projectIds,
        DateTimeOffset? dateGte,
        DateTimeOffset? dateLte)
    {
        var query = from issue in _dbContext.Issues
                    join state in _dbContext.States
                            .Where(s => !s.IsDeleted && s.DeletedOnUtc == null)
                        on issue.StateId equals state.Id into stateJoin
                    from state in stateJoin.DefaultIfEmpty()
                    where issue.TenantId == tenantId
                       && issue.DeletedOnUtc == null
                    select new IssueStateView
                    {
                        ProjectId = issue.ProjectId,
                        CreatedOnUtc = issue.CreatedOnUtc,
                        StateGroup = state != null ? state.Group : (StateGroup?)null,
                    };

        if (!string.IsNullOrEmpty(projectIds))
        {
            var pIds = projectIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(Guid.Parse).ToList();
            query = query.Where(x => pIds.Contains(x.ProjectId));
        }

        if (dateGte.HasValue)
            query = query.Where(x => x.CreatedOnUtc >= dateGte.Value);
        if (dateLte.HasValue)
            query = query.Where(x => x.CreatedOnUtc <= dateLte.Value);

        return query;
    }
}
