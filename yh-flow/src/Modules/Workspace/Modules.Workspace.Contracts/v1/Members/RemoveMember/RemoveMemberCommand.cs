using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.Workspace.Contracts.v1.Members.RemoveMember;

/// <summary>
/// Remove (deactivate) a member from the resolved workspace (REQ-2.2). Admin-driven; the
/// handler rejects self-removal (use <c>LeaveWorkspaceCommand</c> instead).
/// </summary>
public sealed class RemoveMemberCommand : ICommand
{
    /// <summary>Resolved workspace id — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid WorkspaceId { get; set; }

    /// <summary>Member id (route parameter) to remove.</summary>
    public Guid MemberId { get; set; }

    /// <summary>Authenticated user id — set by the endpoint; used for the self-removal guard.</summary>
    [JsonIgnore]
    public Guid CurrentUserId { get; set; }
}
