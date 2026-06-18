using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.Workspace.Contracts.v1.Members.LeaveWorkspace;

/// <summary>
/// Self-removal — the current user leaves the resolved workspace (REQ-2.2). Any active member
/// may call this; it deactivates their own membership row.
/// </summary>
public sealed class LeaveWorkspaceCommand : ICommand
{
    /// <summary>Resolved workspace id — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid WorkspaceId { get; set; }

    /// <summary>Authenticated user id — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid CurrentUserId { get; set; }
}
