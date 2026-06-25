using YH.Framework.Core.Domain;

namespace YH.Modules.View.Domain;

/// <summary>
/// User-scoped favorite tracking for views (tenant-scoped via IHasTenant).
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> this entity is NOT <see cref="IGlobalEntity"/>; <c>BaseDbContext</c>
/// auto-applies <c>IsMultiTenant()</c>, so Finbuckle injects the <see cref="TenantId"/> property.
/// The conditional unique index <c>(TenantId, ViewId, UserId)</c> prevents duplicate favorites.
/// </remarks>
public sealed class ViewFavorite : IHasTenant, ISoftDeletable
{
    public Guid Id { get; private set; }

    /// <summary>View id this favorite references.</summary>
    public Guid ViewId { get; private set; }

    /// <summary>User id who favorited (scalar string, no FK to Identity).</summary>
    public string UserId { get; private set; } = default!;

    /// <summary>When this favorite was created.</summary>
    public DateTimeOffset CreatedOnUtc { get; private set; }

    /// <summary>Finbuckle-managed tenant id (workspace Guid as string).</summary>
    public string TenantId { get; private set; } = default!;

    // ISoftDeletable.
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    private ViewFavorite() { } // EF Core

    /// <summary>
    /// Factory — creates a new ViewFavorite.
    /// </summary>
    /// <param name="viewId">View id (non-empty).</param>
    /// <param name="userId">User id (non-empty).</param>
    public static ViewFavorite Create(Guid viewId, string userId)
    {
        if (viewId == Guid.Empty)
            throw new ArgumentException("View id is required.", nameof(viewId));
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User id is required.", nameof(userId));

        return new ViewFavorite
        {
            Id = Guid.NewGuid(),
            ViewId = viewId,
            UserId = userId,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>Soft-deletes this favorite.</summary>
    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = now;
    }
}