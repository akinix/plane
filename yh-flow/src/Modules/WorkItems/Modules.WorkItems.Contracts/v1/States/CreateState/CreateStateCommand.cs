using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.States.CreateState;

/// <summary>
/// Create a project state (REQ-4.1). <see cref="ProjectId"/> is populated from the route by the endpoint.
/// </summary>
public sealed class CreateStateCommand : ICommand<CreateStateResponse>
{
    /// <summary>Display name (max 255, non-empty).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Optional hex color code (e.g. "#F59E0B"). Max 7 chars.</summary>
    public string? Color { get; set; }

    /// <summary>StateGroup enum integer value (Backlog=0/Unstarted=1/Started=2/Completed=3/Cancelled=4).</summary>
    public int Group { get; set; }

    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint, not client-writable.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }
}
