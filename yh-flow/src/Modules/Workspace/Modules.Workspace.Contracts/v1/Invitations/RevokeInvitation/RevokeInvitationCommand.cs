using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.Workspace.Contracts.v1.Invitations.RevokeInvitation;

/// <summary>
/// Revoke a pending invitation (REQ-2.4). Admin-driven; the handler stamps
/// <c>WorkspaceInvitation.RespondedAt</c>, after which the invitation is no longer actionable
/// (token validation returns null).
/// </summary>
public sealed class RevokeInvitationCommand : ICommand
{
    /// <summary>Resolved workspace id — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid WorkspaceId { get; set; }

    /// <summary>Invitation id (route parameter) to revoke.</summary>
    public Guid InvitationId { get; set; }

    /// <summary>Optional reason recorded in the invitation audit message.</summary>
    public string? Reason { get; set; }
}
