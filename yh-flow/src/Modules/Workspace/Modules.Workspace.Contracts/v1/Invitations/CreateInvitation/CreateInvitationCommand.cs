using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.Workspace.Contracts;

namespace YH.Modules.Workspace.Contracts.v1.Invitations.CreateInvitation;

/// <summary>
/// Invite a user to the resolved workspace by email (REQ-2.4, CONTEXT D-12).
/// </summary>
/// <remarks>
/// The handler generates a CSPRNG raw token, persists ONLY its SHA-256 hash, and returns the
/// raw token in <see cref="CreateInvitationResponse"/> exactly once. The caller (admin) is
/// responsible for delivering the invitation link containing the raw token to the invitee
/// (Phase 11 <c>INotificationService</c> automates email dispatch; Phase 2 leaves it null).
/// </remarks>
public sealed class CreateInvitationCommand : ICommand<CreateInvitationResponse>
{
    /// <summary>Resolved workspace id — set by the endpoint from ICurrentWorkspaceContext.</summary>
    [JsonIgnore]
    public Guid WorkspaceId { get; set; }

    /// <summary>Resolved workspace slug — used to build the invitation link.</summary>
    [JsonIgnore]
    public string Slug { get; set; } = default!;

    /// <summary>Invitee email address.</summary>
    public string Email { get; set; } = default!;

    /// <summary>Role to grant on acceptance (Guest/Member/Admin; None rejected).</summary>
    public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;

    /// <summary>Optional personal message from the inviter.</summary>
    public string? Message { get; set; }
}
