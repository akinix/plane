namespace YH.Modules.Workspace.Contracts.DTOs;

/// <summary>
/// Workspace invitation response DTO (REQ-2.4).
/// Field set mirrors Plane <c>WorkspaceMemberInvite</c> serializer. The raw invitation token is
/// <b>never</b> serialized here (D-12: only the SHA-256 hash is persisted; raw token returned to the
/// caller exactly once at creation time via the <c>CreateInvitationResponse</c>). DTO consumers that
/// need the invitation link read the token from the create-response, not from this listing DTO.
/// </summary>
public class WorkspaceInvitationDto
{
    public Guid Id { get; set; }

    /// <summary>Invitee email address.</summary>
    public string Email { get; set; } = default!;

    /// <summary>Role offered to the invitee on acceptance (numeric 20/15/5 per D-11).</summary>
    public int Role { get; set; }

    /// <summary>Whether the invitation has been accepted.</summary>
    public bool Accepted { get; set; }

    /// <summary>When the invitee responded (accept/reject); null while pending.</summary>
    public DateTimeOffset? RespondedAt { get; set; }

    /// <summary>Invitation message text set by the inviter (optional).</summary>
    public string? Message { get; set; }

    /// <summary>When the invitation was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Hard expiry timestamp (D-12 TTL, default 7 days). After expiry the token is rejected.</summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>Workspace id this invitation belongs to (tenant of the row).</summary>
    public Guid WorkspaceId { get; set; }

    /// <summary>Id of the user who created the invitation (audit; populated from <c>ICurrentUser</c>).</summary>
    public Guid? CreatedBy { get; set; }
}
