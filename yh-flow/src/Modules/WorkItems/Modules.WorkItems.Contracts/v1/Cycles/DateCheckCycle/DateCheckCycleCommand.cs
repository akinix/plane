using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.Cycles.DateCheckCycle;

/// <summary>
/// Checks if the given date range overlaps with any existing cycle in the project.
/// Plane-style triple overlap detection.
/// </summary>
public sealed class DateCheckCycleCommand : ICommand<DateCheckCycleResponse>
{
    /// <summary>Start date of the range to check.</summary>
    public DateTimeOffset StartDate { get; set; }

    /// <summary>End date of the range to check.</summary>
    public DateTimeOffset EndDate { get; set; }

    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Optional cycle id to exclude from overlap check (for updates).</summary>
    public Guid? ExcludeCycleId { get; set; }
}
