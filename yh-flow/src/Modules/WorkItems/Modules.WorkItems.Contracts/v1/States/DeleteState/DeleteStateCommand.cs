using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.States.DeleteState;

/// <summary>
/// Soft-delete a project state (REQ-4.1). <see cref="ProjectId"/> and <see cref="StateId"/> are populated from the route.
/// </summary>
public sealed class DeleteStateCommand : ICommand
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment <c>{stateId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid StateId { get; set; }
}
