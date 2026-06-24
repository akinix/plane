using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.States.ListStates;

/// <summary>
/// List states in a project (REQ-4.1), ordered by SortOrder ascending.
/// Supports optional Group filter.
/// </summary>
public sealed class ListStatesQuery : IQuery<List<StateDto>>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Optional filter by StateGroup integer value (Backlog=0/Unstarted=1/Started=2/Completed=3/Cancelled=4).</summary>
    public int? Group { get; set; }
}
