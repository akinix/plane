namespace YH.Modules.Workspace.Contracts.v1.Invitations.CreateInvitation;

/// <summary>
/// Response to <see cref="CreateInvitationCommand"/> (REQ-2.4, CONTEXT D-12).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Token"/> is the RAW invitation token — returned EXACTLY ONCE at creation time and
/// NEVER persisted in plaintext. The database stores only the SHA-256 hash (see the
/// <c>WorkspaceInvitation.TokenHash</c> column in the Workspace runtime module). The frontend
/// composes the invitation URL as <c>/{slug}/invitation?token={Token}</c>.
/// </para>
/// <para>
/// <see cref="Slug"/> is included so the client can build the link without re-fetching the
/// workspace. <see cref="InvitationId"/> lets the client track the invitation (e.g. revoke it
/// later) — listing endpoints return the same id but NOT the raw token.
/// </para>
/// </remarks>
public sealed record CreateInvitationResponse(Guid InvitationId, string Token, string Slug);
