using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.Cycles.TransferCycleIssues;

/// <summary>
/// Transfer uncompleted issues from the current cycle to another cycle.
/// <see cref="ProjectId"/> and <see cref="CycleId"/> (source cycle) are populated from the route.
/// </summary>
public sealed class TransferCycleIssuesCommand : ICommand<Unit>
{
    /// <summary>Target cycle to transfer issues to.</summary>
    public Guid NewCycleId { get; set; }

    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment <c>{cycleId}</c> (source cycle) — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid CycleId { get; set; }
}
