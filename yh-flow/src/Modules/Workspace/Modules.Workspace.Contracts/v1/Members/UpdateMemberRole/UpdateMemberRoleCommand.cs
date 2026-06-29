using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Contracts.DTOs;

namespace YH.Modules.Workspace.Contracts.v1.Members.UpdateMemberRole;

/// <summary>
/// Update a member's role in the resolved workspace (REQ-2.2, threat T-2-eop-self [BLOCKING]).
/// </summary>
/// <remarks>
/// <see cref="WorkspaceId"/> and <see cref="CurrentUserId"/> are populated by the endpoint
/// (not client-writable). The handler enforces the self-promotion guard (Member cannot
/// promote themselves to Admin) before delegating to <c>WorkspaceMembershipService</c>.
/// </remarks>
public sealed class UpdateMemberRoleCommand : ICommand<WorkspaceMemberDto>
{
    /// <summary>Resolved workspace id — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid WorkspaceId { get; set; }

    /// <summary>Member id (route parameter) whose role is being updated.</summary>
    public Guid MemberId { get; set; }

    /// <summary>New role. Must be Guest/Member/Admin (not None).</summary>
    public WorkspaceRole Role { get; set; }

    /// <summary>Authenticated user id — set by the endpoint; used for the self-promotion guard.</summary>
    [JsonIgnore]
    public Guid CurrentUserId { get; set; }
}
