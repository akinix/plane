using YH.Framework.Core.Domain;

namespace YH.Modules.Workspace.Domain;

/// <summary>
/// Pending workspace invitation sent to an email address (CONTEXT D-12 / threat T-2-softdelete).
/// </summary>
/// <remarks>
/// <b>Token storage (D-12):</b> only the SHA-256 hash of the raw invitation token is persisted
/// (<see cref="TokenHash"/>). The raw token is returned ONCE at create time and never stored —
/// mirrors the Phase 1 <c>APIToken</c> pattern (<c>ApiTokenService.cs:175-187</c> crypto helpers).
/// Lookups at the accept endpoint hash the incoming token and query by <see cref="TokenHash"/>.
/// <para>
/// <b>State machine:</b> a fresh invitation is <see cref="IsValid"/> (not yet accepted, not
/// responded-to, not expired). <see cref="Accept"/> / <see cref="Reject"/> / <see cref="Revoke"/>
/// are terminal transitions — all set <see cref="RespondedAt"/>; <see cref="Accept"/> additionally
/// sets <see cref="Accepted"/> = true. Once responded the invitation can no longer be acted on
/// (the accept endpoint validates <see cref="IsValid"/> before consuming the token).
/// </para>
/// <para>
/// <b>Tenant isolation:</b> auto-scoped by Finbuckle (workspace id is the tenant). The
/// <c>(TenantId, Accepted)</c> composite index supports the "list pending invitations for this
/// workspace" query (Phase 2 plan 02-05 ListInvitations handler).
/// </para>
/// </remarks>
public sealed class WorkspaceInvitation : IHasDomainEvents, IHasTenant, ISoftDeletable, IAuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; private set; }

    /// <summary>Workspace id this invitation targets. Also the Finbuckle tenant id.</summary>
    public Guid WorkspaceId { get; private set; }

    /// <summary>Finbuckle-managed tenant id (workspace Guid as string).</summary>
    public string TenantId { get; private set; } = default!;

    /// <summary>Invitee email. Lowercased at create time (validation in handler/service layer).</summary>
    public string Email { get; private set; } = default!;

    /// <summary>
    /// SHA-256 hex hash of the raw invitation token (D-12). MaxLength=64 (SHA-256 hex = 64 chars).
    /// Unique index (lookups by hash at the accept endpoint).
    /// </summary>
    public string TokenHash { get; private set; } = default!;

    /// <summary>Role code the invitee will receive on accept (Guest=5 / Member=15 / Admin=20).</summary>
    public int Role { get; private set; }

    /// <summary>True once <see cref="Accept"/> has fired; false for fresh / rejected / revoked.</summary>
    public bool Accepted { get; private set; }

    /// <summary>
    /// Timestamp of the most recent terminal response (Accept / Reject / Revoke). Null on a fresh,
    /// un-responded invitation. Used by <see cref="IsValid"/> to gate the accept endpoint.
    /// </summary>
    public DateTimeOffset? RespondedAt { get; private set; }

    /// <summary>Optional personal message from the inviter.</summary>
    public string? Message { get; private set; }

    /// <summary>
    /// Hard expiry timestamp. <see cref="IsExpired"/> becomes true after this instant regardless of
    /// <see cref="RespondedAt"/>. <see cref="IsValid"/> is false past expiry.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; private set; }

    // IAuditableEntity
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }

    // ISoftDeletable
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    private WorkspaceInvitation() { } // EF Core

    /// <summary>
    /// Factory — creates a new pending invitation. Caller generates the raw token, hashes it,
    /// and passes only the hash here (D-12 — raw token never enters persistence).
    /// </summary>
    /// <param name="workspaceId">Target workspace id.</param>
    /// <param name="email">Invitee email.</param>
    /// <param name="tokenHash">SHA-256 hex hash of the raw invitation token.</param>
    /// <param name="role">Role to grant on accept.</param>
    /// <param name="ttlDays">Lifetime in days (added to UtcNow to compute <see cref="ExpiresAt"/>).</param>
    /// <param name="message">Optional personal message.</param>
    public static WorkspaceInvitation Create(
        Guid workspaceId,
        string email,
        string tokenHash,
        int role,
        int ttlDays,
        string? message = null)
    {
        if (workspaceId == Guid.Empty)
            throw new ArgumentException("Workspace id is required.", nameof(workspaceId));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Token hash is required.", nameof(tokenHash));
        if (ttlDays <= 0)
            throw new ArgumentOutOfRangeException(nameof(ttlDays), "TTL must be positive.");

        return new WorkspaceInvitation
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Email = email,
            TokenHash = tokenHash,
            Role = role,
            Accepted = false,
            RespondedAt = null,
            Message = message,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(ttlDays),
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// True when <see cref="ExpiresAt"/> is in the past. Independent of <see cref="RespondedAt"/>.
    /// </summary>
    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;

    /// <summary>
    /// True iff the invitation is still actionable: not yet accepted, not yet responded to
    /// (rejected/revoked), and not past <see cref="ExpiresAt"/>. The accept endpoint asserts this
    /// before consuming the token (D-12). Once any terminal transition fires <see cref="IsValid"/>
    /// becomes false and stays false.
    /// </summary>
    public bool IsValid => !Accepted && RespondedAt is null && !IsExpired;

    /// <summary>
    /// Terminal transition — invitee accepted. Sets <see cref="Accepted"/> = true and stamps
    /// <see cref="RespondedAt"/>. Caller is responsible for creating the WorkspaceMember row in
    /// the same unit of work (plan 02-05 AcceptInvitation handler).
    /// </summary>
    public bool Accept()
    {
        if (RespondedAt is not null || Accepted) return false; // already terminal
        Accepted = true;
        MarkResponded();
        return true;
    }

    /// <summary>Terminal transition — invitee explicitly rejected.</summary>
    public bool Reject()
    {
        if (RespondedAt is not null) return false;
        Accepted = false;
        MarkResponded();
        return true;
    }

    /// <summary>
    /// Terminal transition — admin/inviter revoked the invitation. Records an optional
    /// <paramref name="reason"/> for audit (Plane surfaces this in the invitations admin UI;
    /// distinct from <see cref="Reject"/> which is invitee-initiated and carries no reason).
    /// </summary>
    public bool Revoke(string? reason = null)
    {
        if (RespondedAt is not null) return false;
        Accepted = false;
        // Revoke carries a reason; Reject is invitee-driven and reason-less. This keeps Revoke's
        // body distinct from Reject so the domain model records the actor difference (Sonar S4144).
        if (!string.IsNullOrWhiteSpace(reason))
        {
            Message = string.IsNullOrWhiteSpace(Message)
                ? $"[revoked: {reason}]"
                : $"{Message} [revoked: {reason}]";
        }
        MarkResponded();
        return true;
    }

    private void MarkResponded()
    {
        var now = DateTimeOffset.UtcNow;
        RespondedAt = now;
        LastModifiedOnUtc = now;
    }
}
