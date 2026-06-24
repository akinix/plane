using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Services;

/// <summary>
/// Default implementation of <see cref="IBurndownCalculator"/>.
/// Implements completion chart calculation using Pattern 4 algorithm (date-sequence + cumulative subtraction).
/// Full implementation depends on Cycle/CycleIssue DbSets added in Task 2.
/// </summary>
public sealed class BurndownCalculator : IBurndownCalculator
{
#pragma warning disable S4487 // Unused private field — will be used in Task 2 full implementation
    private readonly WorkItemsDbContext _db;
#pragma warning restore S4487

    public BurndownCalculator(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    /// <inheritdoc />
    public Task<Dictionary<string, int?>> CalculateCompletionChartAsync(
        Guid cycleId, string type, CancellationToken cancellationToken)
    {
        // Stub: full implementation requires Cycle/CycleIssue entities (Task 2)
        return Task.FromResult<Dictionary<string, int?>>([]);
    }

    /// <inheritdoc />
    public Task<CycleProgressSnapshot> BuildSnapshotAsync(
        Guid cycleId, CancellationToken cancellationToken)
    {
        // Stub: full implementation requires Cycle/CycleIssue entities (Task 2)
        return Task.FromResult(new CycleProgressSnapshot(0, 0, 0, 0, 0, 0, []));
    }
}
