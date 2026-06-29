using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.States.UpdateState;

/// <summary>
/// Update mutable state display fields (REQ-4.1). All fields are optional (PATCH semantics).
/// <see cref="ProjectId"/> and <see cref="StateId"/> are populated from the route.
/// </summary>
public sealed class UpdateStateCommand : ICommand<StateDto>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment <c>{stateId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid StateId { get; set; }

    /// <summary>Display name (max 255).</summary>
    public string? Name { get; set; }

    /// <summary>Optional hex color code (e.g. "#F59E0B").</summary>
    public string? Color { get; set; }

    /// <summary>Sort order for state listing UI.</summary>
    public double? SortOrder { get; set; }
}
