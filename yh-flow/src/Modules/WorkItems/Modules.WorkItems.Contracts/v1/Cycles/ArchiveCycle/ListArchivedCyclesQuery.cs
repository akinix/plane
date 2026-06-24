using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Cycles.ArchiveCycle;

/// <summary>
/// List archived cycles in a project. <see cref="ProjectId"/> is populated from the route.
/// </summary>
public sealed class ListArchivedCyclesQuery : IQuery<List<CycleDto>>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }
}
