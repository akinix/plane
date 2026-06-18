using Mediator;

namespace YH.Modules.Workspace.Contracts.v1.Invitations.RejectInvitation;

/// <summary>
/// Reject a workspace invitation (REQ-2.4). Invitee-driven; stamps
/// <c>WorkspaceInvitation.RespondedAt</c> (no member row is created).
/// </summary>
public sealed class RejectInvitationCommand : ICommand<RejectInvitationResponse>
{
    /// <summary>Raw invitation token (from URL path).</summary>
    public string Token { get; set; } = default!;
}

/// <summary>Result of <see cref="RejectInvitationCommand"/>.</summary>
public sealed record RejectInvitationResponse(bool Success);
