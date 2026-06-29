using Microsoft.EntityFrameworkCore;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Services;

/// <summary>
/// Default implementation of <see cref="IBurndownCalculator"/>.
/// Implements completion chart calculation using Pattern 4 algorithm
/// (date-sequence generation + cumulative subtraction per day).
/// </summary>
public sealed class BurndownCalculator : IBurndownCalculator
{
    private readonly WorkItemsDbContext _db;

    public BurndownCalculator(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, int?>> CalculateCompletionChartAsync(
        Guid cycleId, string type, CancellationToken cancellationToken)
    {
        // 1) Load the cycle to determine date range
        var cycle = await _db.Cycles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == cycleId, cancellationToken)
            .ConfigureAwait(false);

        if (cycle is null || cycle.StartDate is null || cycle.EndDate is null)
            return [];

        // 2) Determine date range
        var today = DateTimeOffset.UtcNow.Date;
        var endDate = cycle.EndDate.Value.Date > today
            ? today
            : cycle.EndDate.Value.Date;
        var startDate = cycle.StartDate.Value.Date;

        // 3) Generate date sequence
        var dateSequence = new List<DateOnly>();
        var current = startDate;
        while (current <= endDate)
        {
            dateSequence.Add(DateOnly.FromDateTime(current));
            current = current.AddDays(1);
        }

        // 4) Query completed issue IDs in this cycle
        var cycleIssueIds = await _db.CycleIssues
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(ci => ci.CycleId == cycleId)
            .Select(ci => ci.IssueId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (cycleIssueIds.Count == 0)
        {
            // No issues in cycle — empty chart
            var emptyResult = new Dictionary<string, int?>();
            foreach (var date in dateSequence)
                emptyResult[date.ToString("yyyy-MM-dd")] = null;
            return emptyResult;
        }

        // 5) Query completed dates for issues in this cycle
        var completedDates = await _db.Issues
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(i => cycleIssueIds.Contains(i.Id) && i.CompletedAt.HasValue)
            .Select(i => i.CompletedAt!.Value)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // 6) Group by date in memory
        var countsByDate = completedDates
            .GroupBy(d => DateOnly.FromDateTime(d.DateTime))
            .ToDictionary(g => g.Key, g => g.Count());

        // 7) Build cumulative completion chart
        var cumulative = 0;
        var result = new Dictionary<string, int?>();

        foreach (var date in dateSequence)
        {
            if (countsByDate.TryGetValue(date, out var dayCount))
                cumulative += dayCount;

            result[date.ToString("yyyy-MM-dd")] = cumulative;
        }

        // 8) Future dates beyond today as null
        if (cycle.EndDate.Value.Date > today)
        {
            var future = today.AddDays(1);
            while (future <= cycle.EndDate.Value.Date)
            {
                result[future.ToString("yyyy-MM-dd")] = null;
                future = future.AddDays(1);
            }
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<CycleProgressSnapshot> BuildSnapshotAsync(
        Guid cycleId, CancellationToken cancellationToken)
    {
        // 1) Get issue IDs in this cycle
        var cycleIssueIds = await _db.CycleIssues
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(ci => ci.CycleId == cycleId)
            .Select(ci => ci.IssueId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var totalIssues = cycleIssueIds.Count;

        // 2) Query issue stats with state group info
        var issuesWithState = await _db.Issues
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(i => cycleIssueIds.Contains(i.Id))
            .Join(
                _db.States.AsNoTracking().IgnoreQueryFilters(),
                i => i.StateId,
                s => s.Id,
                (i, s) => new { i.CompletedAt, s.Group })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var completedIssues = issuesWithState.Count(x => x.CompletedAt.HasValue && x.Group == StateGroup.Completed);
        var cancelledIssues = issuesWithState.Count(x => x.Group == StateGroup.Cancelled);
        var startedIssues = issuesWithState.Count(x => x.Group == StateGroup.Started);
        var unstartedIssues = issuesWithState.Count(x => x.Group == StateGroup.Unstarted);
        var backlogIssues = issuesWithState.Count(x => x.Group == StateGroup.Backlog);

        // 3) Calculate completion chart
        var chart = await CalculateCompletionChartAsync(cycleId, "completion", cancellationToken)
            .ConfigureAwait(false);

        return new CycleProgressSnapshot(
            TotalIssues: totalIssues,
            CompletedIssues: completedIssues,
            CancelledIssues: cancelledIssues,
            StartedIssues: startedIssues,
            UnstartedIssues: unstartedIssues,
            BacklogIssues: backlogIssues,
            CompletionChart: chart);
    }
}
