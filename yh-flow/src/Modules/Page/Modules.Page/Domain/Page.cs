using YH.Framework.Core.Domain;

namespace YH.Modules.Page.Domain;

/// <summary>
/// Page access visibility enum.
/// Mirrors Plane <c>models/page.py</c> PageAccess.
/// </summary>
public enum PageAccess
{
    /// <summary>Public page — visible to all workspace members.</summary>
    Public = 0,

    /// <summary>Private page — only visible to the owner (<see cref="Page.OwnedBy"/>).</summary>
    Private = 1,
}

/// <summary>
/// Page aggregate — a document page within a project (Plane <c>models/page.py</c>).
/// </summary>
/// <remarks>
/// <b>Tenant isolation (CONTEXT.md Claude's Discretion):</b>
/// <c>Page</c> is <see cref="IGlobalEntity"/> — it deliberately OPTS OUT of Finbuckle's
/// <c>IsMultiTenant()</c> auto-application. Tenant is resolved via workspace slug in the route,
/// not via a TenantId column. <c>BaseDbContext.OnModelCreating</c>
/// → <c>ApplyTenantIsolationByDefault()</c> skips any <c>IGlobalEntity</c>.
/// <para>
/// <b>Soft delete:</b> standard ISoftDeletable pattern. No slug rewrite needed — Page
/// has no slug or identifier field that requires unconditional unique index release.
/// </para>
/// <para>
/// <b>Cross-module references (D-04/D-06):</b> <see cref="ProjectId"/> and <see cref="OwnedBy"/>
/// are scalar <c>Guid</c> references with NO foreign key to Project or Identity modules.
/// Referential integrity is enforced at the application layer.
/// </para>
/// </remarks>
public sealed class Page : IGlobalEntity, ISoftDeletable, IAuditableEntity
{
    public Guid Id { get; private set; }

    /// <summary>Display name (Plane: max 255).</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Optional HTML description content (Plane: unlimited, nvarchar(max)).</summary>
    public string? DescriptionHtml { get; private set; }

    /// <summary>Auto-generated plain-text version of DescriptionHtml.</summary>
    public string? DescriptionStripped { get; private set; }

    /// <summary>Optional JSON description content (editor state sync).</summary>
    public string? DescriptionJson { get; private set; }

    /// <summary>Visibility: <see cref="PageAccess.Public"/> (0) or <see cref="PageAccess.Private"/> (1). Defaults to Public.</summary>
    public PageAccess Access { get; private set; }

    /// <summary>Optional hex color for page identification.</summary>
    public string? Color { get; private set; }

    /// <summary>Custom sort order for page listing UI. Default 65535.0 (Plane convention).</summary>
    public double SortOrder { get; private set; } = 65535.0;

    /// <summary>Whether the page is locked for editing.</summary>
    public bool IsLocked { get; private set; }

    /// <summary>Optional parent page id (self-referencing FK, for hierarchy/tree nesting).</summary>
    public Guid? ParentId { get; private set; }

    /// <summary>Owner user id (scalar Guid, no cross-module FK per D-06).</summary>
    public Guid OwnedBy { get; private set; }

    /// <summary>Timestamp when archived (null = active).</summary>
    public DateTimeOffset? ArchivedAt { get; private set; }

    /// <summary>JSON view configuration props.</summary>
    public string? ViewProps { get; private set; }

    /// <summary>JSON logo configuration props.</summary>
    public string? LogoProps { get; private set; }

    /// <summary>Whether this page is shared across the workspace.</summary>
    public bool IsGlobal { get; private set; }

    /// <summary>Optional external source identifier (for Plane import/API compatibility).</summary>
    public string? ExternalSource { get; private set; }

    /// <summary>Optional external record id (for Plane import/API compatibility).</summary>
    public string? ExternalId { get; private set; }

    /// <summary>Scalar project id (cross-module reference, no navigation property).</summary>
    public Guid ProjectId { get; private set; }

    // IAuditableEntity — populated by AuditableEntitySaveChangesInterceptor.
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }

    // ISoftDeletable.
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    private Page() { } // EF Core

    /// <summary>
    /// Factory — creates a new active <see cref="Page"/>.
    /// </summary>
    /// <param name="name">Display name (non-empty, max 255).</param>
    /// <param name="projectId">Project id (non-empty).</param>
    /// <param name="ownedBy">Owner user id (non-empty).</param>
    /// <param name="descriptionHtml">Optional HTML description content.</param>
    /// <param name="descriptionStripped">Optional plain-text description.</param>
    /// <param name="descriptionJson">Optional JSON description content.</param>
    /// <param name="access">Visibility (default <see cref="PageAccess.Public"/>).</param>
    /// <param name="color">Optional hex color.</param>
    /// <param name="parentId">Optional parent page id (for hierarchy).</param>
    /// <param name="sortOrder">Optional sort order (default 65535.0).</param>
    /// <param name="viewProps">Optional JSON view config.</param>
    /// <param name="logoProps">Optional JSON logo config.</param>
    /// <param name="isGlobal">Whether this page is workspace-wide visible.</param>
    /// <param name="externalSource">Optional external source identifier.</param>
    /// <param name="externalId">Optional external record id.</param>
    public static Page Create(
        string name,
        Guid projectId,
        Guid ownedBy,
        string? descriptionHtml = null,
        string? descriptionStripped = null,
        string? descriptionJson = null,
        PageAccess access = PageAccess.Public,
        string? color = null,
        Guid? parentId = null,
        double sortOrder = 65535.0,
        string? viewProps = null,
        string? logoProps = null,
        bool isGlobal = false,
        string? externalSource = null,
        string? externalId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Page name is required.", nameof(name));
        if (projectId == Guid.Empty)
            throw new ArgumentException("Project id is required.", nameof(projectId));
        if (ownedBy == Guid.Empty)
            throw new ArgumentException("Owner user id is required.", nameof(ownedBy));

        return new Page
        {
            Id = Guid.NewGuid(),
            Name = name,
            ProjectId = projectId,
            OwnedBy = ownedBy,
            DescriptionHtml = descriptionHtml,
            DescriptionStripped = descriptionStripped,
            DescriptionJson = descriptionJson,
            Access = access,
            Color = color,
            ParentId = parentId,
            SortOrder = sortOrder,
            ViewProps = viewProps,
            LogoProps = logoProps,
            IsGlobal = isGlobal,
            ExternalSource = externalSource,
            ExternalId = externalId,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Updates mutable display fields. PATCH semantics — only non-null parameters are applied.
    /// </summary>
    public void Update(
        string? name = null,
        string? descriptionHtml = null,
        string? descriptionStripped = null,
        string? descriptionJson = null,
        PageAccess? access = null,
        string? color = null,
        Guid? parentId = null,
        double? sortOrder = null,
        string? viewProps = null,
        string? logoProps = null,
        bool? isGlobal = null)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name;
        if (descriptionHtml is not null) DescriptionHtml = descriptionHtml;
        if (descriptionStripped is not null) DescriptionStripped = descriptionStripped;
        if (descriptionJson is not null) DescriptionJson = descriptionJson;
        if (access.HasValue) Access = access.Value;
        if (color is not null) Color = color;
        if (parentId is not null) ParentId = parentId;
        if (sortOrder.HasValue) SortOrder = sortOrder.Value;
        if (viewProps is not null) ViewProps = viewProps;
        if (logoProps is not null) LogoProps = logoProps;
        if (isGlobal.HasValue) IsGlobal = isGlobal.Value;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Archives the page. An archived page is hidden from default views
    /// but its data is preserved (inverse of <see cref="Unarchive"/>).
    /// </summary>
    public void Archive()
    {
        if (ArchivedAt is not null) return;
        ArchivedAt = DateTimeOffset.UtcNow;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Restores an archived page to active status.</summary>
    public void Unarchive()
    {
        if (ArchivedAt is null) return;
        ArchivedAt = null;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Soft-deletes this page. No slug rewrite needed — Page has no slug field.
    /// </summary>
    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = now;
        LastModifiedOnUtc = now;
    }

    /// <summary>
    /// Updates the description content fields.
    /// </summary>
    public void UpdateDescription(string? descriptionHtml, string? descriptionStripped, string? descriptionJson = null)
    {
        if (descriptionHtml is not null) DescriptionHtml = descriptionHtml;
        if (descriptionStripped is not null) DescriptionStripped = descriptionStripped;
        if (descriptionJson is not null) DescriptionJson = descriptionJson;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Updates the sort order value for page listing UI.</summary>
    public void SetSortOrder(double sortOrder)
    {
        SortOrder = sortOrder;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }
}