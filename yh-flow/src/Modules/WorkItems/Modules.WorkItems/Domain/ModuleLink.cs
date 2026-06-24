using YH.Framework.Core.Domain;

namespace YH.Modules.WorkItems.Domain;

/// <summary>
/// Module-Link entity (Plane <c>models/module.py</c> ModuleLink).
/// Represents an external resource linked to a module (e.g. Figma design, document).
/// Soft-deletable for data preservation.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <c>ModuleLink</c> is tenant-scoped via <see cref="IHasTenant"/>.
/// </remarks>
public sealed class ModuleLink : IHasTenant, ISoftDeletable
{
    public Guid Id { get; private set; }

    /// <summary>Link title (max 255, required).</summary>
    public string Title { get; private set; } = default!;

    /// <summary>Link URL (max 2048, required).</summary>
    public string Url { get; private set; } = default!;

    /// <summary>Optional metadata (JSON string).</summary>
    public string? Metadata { get; private set; }

    /// <summary>Module id (FK, cascade delete).</summary>
    public Guid ModuleId { get; private set; }

    /// <summary>When this link was created.</summary>
    public DateTimeOffset CreatedOnUtc { get; private set; }

    // IHasTenant — populated by Finbuckle on save.
    public string TenantId { get; private set; } = default!;

    // ISoftDeletable.
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    private ModuleLink() { } // EF Core

    /// <summary>
    /// Factory — creates a new <see cref="ModuleLink"/>.
    /// </summary>
    public static ModuleLink Create(string title, string url, string? metadata, Guid moduleId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Link title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Link url is required.", nameof(url));
        if (moduleId == Guid.Empty)
            throw new ArgumentException("Module id is required.", nameof(moduleId));

        return new ModuleLink
        {
            Id = Guid.NewGuid(),
            Title = title,
            Url = url,
            Metadata = metadata,
            ModuleId = moduleId,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Updates mutable fields with nullable param PATCH semantics.
    /// </summary>
    public void Update(string? title = null, string? url = null, string? metadata = null)
    {
        if (!string.IsNullOrWhiteSpace(title)) Title = title;

        if (!string.IsNullOrWhiteSpace(url)) Url = url;

        if (metadata is not null)
            Metadata = metadata;
    }

    /// <summary>Soft-deletes this module link (idempotent).</summary>
    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = now;
    }
}
