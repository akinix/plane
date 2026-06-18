using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Domain;

namespace YH.Modules.Workspace.Services;

/// <summary>
/// Invitation token crypto + persistence service (CONTEXT D-12, threat T-2-token [BLOCKING]).
/// </summary>
/// <remarks>
/// <para>
/// <b>Token storage (D-12):</b> the raw token is generated via CSPRNG
/// (<see cref="System.Security.Cryptography.RandomNumberGenerator"/>) and returned to the
/// caller EXACTLY ONCE at creation time. Only its SHA-256 hex hash is persisted to the
/// <c>WorkspaceInvitation.TokenHash</c> column. The accept endpoint hashes the incoming raw
/// token and looks the invitation up by <c>TokenHash</c> — the raw token is never re-read
/// from the database.
/// </para>
/// <para>
/// <b>TTL (threat T-2-ttl):</b> <see cref="CreateAsync"/> stamps <c>ExpiresAt = UtcNow +
/// ttlDays</c> on the invitation. <see cref="ValidateAsync"/> rejects tokens whose
/// invitation is past expiry (delegated to <see cref="WorkspaceInvitation.IsValid"/>) — the
/// TTL default is configured via <c>Workspace:InvitationTokenTtlDays</c> (default 7 days).
/// </para>
/// </remarks>
public interface IInvitationTokenService
{
    /// <summary>
    /// Generates a fresh (rawToken, hash) pair using the CSPRNG. The raw token is the value
    /// returned to the inviter; the hash is what is persisted. Deterministic length — see
    /// <see cref="InvitationTokenService.TokenHexLength"/>.
    /// </summary>
    (string RawToken, string Hash) GenerateToken();

    /// <summary>
    /// SHA-256 hex hash of a raw token. Deterministic — the same raw token always produces the
    /// same hash, so the accept endpoint can hash the inbound token and look the invitation up
    /// by <see cref="WorkspaceInvitation.TokenHash"/>.
    /// </summary>
    string HashToken(string rawToken);

    /// <summary>
    /// Creates + persists a new pending invitation. The raw token is returned ONCE; only the
    /// hash is stored on the entity. Caller is responsible for handing the raw token to the
    /// notification layer (Phase 11 <c>INotificationService</c>) — it is never re-readable.
    /// </summary>
    /// <param name="workspaceId">Target workspace id (also the Finbuckle tenant).</param>
    /// <param name="email">Invitee email.</param>
    /// <param name="role">Role to grant on accept.</param>
    /// <param name="ttlDays">Lifetime in days (added to UtcNow to compute ExpiresAt).</param>
    /// <param name="message">Optional personal message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The persisted invitation (carrying TokenHash only) + the raw token.</returns>
    Task<(WorkspaceInvitation Invitation, string RawToken)> CreateAsync(
        Guid workspaceId,
        string email,
        WorkspaceRole role,
        int ttlDays,
        string? message,
        CancellationToken cancellationToken);

    /// <summary>
    /// Validates a raw token by hashing it and looking up the matching invitation. Returns null
    /// when the hash does not match any invitation OR the matched invitation is no longer
    /// <see cref="WorkspaceInvitation.IsValid"/> (accepted / responded / expired). The returned
    /// entity is untracked — callers that need to mutate (Accept/Reject/Revoke) must
    /// re-attach or re-query tracked.
    /// </summary>
    Task<WorkspaceInvitation?> ValidateAsync(string rawToken, CancellationToken cancellationToken);
}
