using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.States.GetState;

/// <summary>
/// Fetch a single state by id (REQ-4.1). <see cref="ProjectId"/> and <see cref="StateId"/> are populated from the route.
/// </summary>
public sealed class GetStateQuery : IQuery<StateDto>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment <c>{stateId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid StateId { get; set; }
}
