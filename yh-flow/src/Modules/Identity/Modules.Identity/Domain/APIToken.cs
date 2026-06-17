using YH.Framework.Core.Domain;

namespace YH.Modules.Identity.Domain;

/// <summary>
/// Represents a user's API key for programmatic access. Stores only the SHA-256
/// hash of the key — the plaintext is returned once at creation and never persisted.
/// Implements <see cref="IHasTenant"/> for multi-tenant data isolation.
/// </summary>
public class APIToken : IHasDomainEvents, IHasTenant
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string UserId { get; private set; } = default!;
    public string TokenHash { get; private set; } = default!;
    public string Prefix { get; private set; } = default!;
    public string TenantId { get; private set; } = default!;
    public DateTime? ExpiredAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastUsed { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation property
    public virtual FshUser? User { get; init; }

    // IHasDomainEvents implementation
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    private APIToken() { } // EF Core

    /// <summary>
    /// Factory method to create a new API token.
    /// </summary>
    public static APIToken Create(
        string name,
        string userId,
        string tokenHash,
        string prefix,
        string tenantId,
        DateTime? expiredAt)
    {
        return new APIToken
        {
            Id = Guid.NewGuid(),
            Name = name,
            UserId = userId,
            TokenHash = tokenHash,
            Prefix = prefix,
            TenantId = tenantId,
            ExpiredAt = expiredAt,
            IsActive = true,
            LastUsed = null,
            CreatedAt = TimeProvider.System.GetUtcNow().UtcDateTime,
        };
    }

    /// <summary>
    /// Records that this token was used, updating the LastUsed timestamp.
    /// </summary>
    public void RecordUsage()
    {
        LastUsed = TimeProvider.System.GetUtcNow().UtcDateTime;
    }

    /// <summary>
    /// Revokes this token, setting IsActive to false.
    /// </summary>
    public void Revoke()
    {
        IsActive = false;
    }

    /// <summary>
    /// Returns true if the token has expired (ExpiredAt is in the past).
    /// Returns false if ExpiredAt is null (never expires) or in the future.
    /// </summary>
    public bool IsExpired()
    {
        return ExpiredAt.HasValue && ExpiredAt.Value < TimeProvider.System.GetUtcNow().UtcDateTime;
    }
}
