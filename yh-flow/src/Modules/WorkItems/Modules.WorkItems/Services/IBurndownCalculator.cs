namespace YH.Modules.WorkItems.Services;

/// <summary>
/// Calculates burndown/completion chart data for cycles.
/// </summary>
public interface IBurndownCalculator
{
    /// <summary>
    /// Calculates the completion chart for a cycle.
    /// Returns a dictionary of date strings (yyyy-MM-dd) to cumulative completed issue counts.
    /// Future dates have null values.
    /// </summary>
    /// <param name="cycleId">The cycle identifier.</param>
    /// <param name="type">Chart type (e.g. "completion").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Dictionary<string, int?>> CalculateCompletionChartAsync(Guid cycleId, string type, CancellationToken cancellationToken);

    /// <summary>
    /// Builds a progress snapshot for a cycle, including issue distribution stats and completion chart.
    /// </summary>
    /// <param name="cycleId">The cycle identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<CycleProgressSnapshot> BuildSnapshotAsync(Guid cycleId, CancellationToken cancellationToken);
}

/// <summary>
/// Immutable snapshot of a cycle's progress state.
/// </summary>
public sealed record CycleProgressSnapshot(
    int TotalIssues,
    int CompletedIssues,
    int CancelledIssues,
    int StartedIssues,
    int UnstartedIssues,
    int BacklogIssues,
    Dictionary<string, int?> CompletionChart);
