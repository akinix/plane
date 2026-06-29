using YH.Framework.Core.Domain;

namespace YH.Modules.Page.Domain;

/// <summary>
/// User-scoped favorite tracking for pages (tenant-scoped via IHasTenant).
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> this entity is NOT <see cref="IGlobalEntity"/>; <c>BaseDbContext</c>
/// auto-applies <c>IsMultiTenant()</c>, so Finbuckle injects the <see cref="TenantId"/> property.
/// The conditional unique index <c>(TenantId, PageId, UserId)</c> prevents duplicate favorites.
/// </remarks>
public sealed class PageFavorite : IHasTenant, ISoftDeletable
{
    public Guid Id { get; private set; }

    /// <summary>Page id this favorite references.</summary>
    public Guid PageId { get; private set; }

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

    private PageFavorite() { } // EF Core

    /// <summary>
    /// Factory — creates a new PageFavorite.
    /// </summary>
    /// <param name="pageId">Page id (non-empty).</param>
    /// <param name="userId">User id (non-empty).</param>
    public static PageFavorite Create(Guid pageId, string userId)
    {
        if (pageId == Guid.Empty)
            throw new ArgumentException("Page id is required.", nameof(pageId));
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User id is required.", nameof(userId));

        return new PageFavorite
        {
            Id = Guid.NewGuid(),
            PageId = pageId,
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