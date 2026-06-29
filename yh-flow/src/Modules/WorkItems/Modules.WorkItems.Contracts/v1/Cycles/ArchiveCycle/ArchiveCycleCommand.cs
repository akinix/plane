using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.Cycles.ArchiveCycle;

/// <summary>
/// Archive a cycle. <see cref="ProjectId"/> and <see cref="CycleId"/> are populated from the route.
/// Only completed cycles can be archived.
/// </summary>
public sealed class ArchiveCycleCommand : ICommand<Unit>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment <c>{cycleId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid CycleId { get; set; }
}
