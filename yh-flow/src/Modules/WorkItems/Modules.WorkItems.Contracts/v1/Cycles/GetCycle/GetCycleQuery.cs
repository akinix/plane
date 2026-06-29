using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Cycles.GetCycle;

/// <summary>
/// Fetch a single cycle by id. <see cref="ProjectId"/> and <see cref="CycleId"/> are populated from the route.
/// </summary>
public sealed class GetCycleQuery : IQuery<CycleDto>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment <c>{cycleId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid CycleId { get; set; }
}
