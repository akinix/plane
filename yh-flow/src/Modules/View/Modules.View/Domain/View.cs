using YH.Framework.Core.Domain;

namespace YH.Modules.View.Domain;

/// <summary>
/// View access visibility enum.
/// Mirrors Plane <c>models/issue_view.py</c> ViewAccess.
/// </summary>
public enum ViewAccess
{
    /// <summary>Private view — only visible to the owner (<see cref="View.OwnedBy"/>).</summary>
    Private = 0,

    /// <summary>Public view — visible to all workspace members.</summary>
    Public = 1,
}

/// <summary>
/// View aggregate — a saved view/filter preset for issues within a project or workspace
/// (Plane <c>models/issue_view.py</c> IssueView).
/// </summary>
/// <remarks>
/// <b>Tenant isolation (CONTEXT.md Claude's Discretion):</b>
/// <c>View</c> is <see cref="IGlobalEntity"/> — it deliberately OPTS OUT of Finbuckle's
/// <c>IsMultiTenant()</c> auto-application. Tenant is resolved via workspace slug in the route,
/// not via a TenantId column. <c>BaseDbContext.OnModelCreating</c>
/// → <c>ApplyTenantIsolationByDefault()</c> skips any <c>IGlobalEntity</c>.
/// <para>
/// <b>Project scope:</b> <see cref="ProjectId"/> is nullable — null = workspace-level view,
/// non-null = project-level view.
/// </para>
/// <para>
/// <b>Cross-module references:</b> <see cref="ProjectId"/> and <see cref="OwnedBy"/>
/// are scalar <c>Guid</c> references with NO foreign key to Project or Identity modules.
/// Referential integrity is enforced at the application layer.
/// </para>
/// <para>
/// <b>No IHasDomainEvents:</b> View does not require activity log auditing per CONTEXT.md.
/// </para>
/// </remarks>
public sealed class View : IGlobalEntity, ISoftDeletable, IAuditableEntity
{
    public Guid Id { get; private set; }

    /// <summary>Display name (Plane: max 255).</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Optional description (Plane: unlimited, nvarchar(max)).</summary>
    public string? Description { get; private set; }

    /// <summary>Auto-generated query string from filters (Plane: issue_filters()).</summary>
    public string? Query { get; private set; }

    /// <summary>JSON dictionary of filter conditions.</summary>
    public string? Filters { get; private set; }

    /// <summary>JSON dictionary of display/grouping settings.</summary>
    public string? DisplayFilters { get; private set; }

    /// <summary>JSON dictionary of column display toggles.</summary>
    public string? DisplayProperties { get; private set; }

    /// <summary>JSON dictionary of advanced (rich) filters (optional).</summary>
    public string? RichFilters { get; private set; }

    /// <summary>Visibility: <see cref="ViewAccess.Private"/> (0) or <see cref="ViewAccess.Public"/> (1). Defaults to Public.</summary>
    public ViewAccess Access { get; private set; }

    /// <summary>Custom sort order for view listing UI. Default 65535.0 (Plane convention).</summary>
    public double SortOrder { get; private set; } = 65535.0;

    /// <summary>JSON logo configuration props.</summary>
    public string? LogoProps { get; private set; }

    /// <summary>Owner user id (scalar Guid, no cross-module FK per D-06).</summary>
    public Guid OwnedBy { get; private set; }

    /// <summary>Whether the view is locked for editing.</summary>
    public bool IsLocked { get; private set; }

    /// <summary>Timestamp when archived (null = active).</summary>
    public DateTimeOffset? ArchivedAt { get; private set; }

    /// <summary>Optional project id (null = workspace-level view).</summary>
    public Guid? ProjectId { get; private set; }

    // IAuditableEntity — populated by AuditableEntitySaveChangesInterceptor.
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }

    // ISoftDeletable.
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    private View() { } // EF Core

    /// <summary>
    /// Factory — creates a new active <see cref="View"/>.
    /// </summary>
    /// <param name="name">Display name (non-empty, max 255).</param>
    /// <param name="ownedBy">Owner user id (non-empty).</param>
    /// <param name="projectId">Optional project id (null = workspace-level view).</param>
    /// <param name="description">Optional description.</param>
    /// <param name="filters">Optional JSON filter conditions.</param>
    /// <param name="displayFilters">Optional JSON display/grouping settings.</param>
    /// <param name="displayProperties">Optional JSON column display toggles.</param>
    /// <param name="richFilters">Optional JSON advanced filters.</param>
    /// <param name="access">Visibility (default <see cref="ViewAccess.Public"/>).</param>
    /// <param name="sortOrder">Optional sort order (default 65535.0).</param>
    /// <param name="logoProps">Optional JSON logo props.</param>
    public static View Create(
        string name,
        Guid ownedBy,
        Guid? projectId = null,
        string? description = null,
        string? filters = null,
        string? displayFilters = null,
        string? displayProperties = null,
        string? richFilters = null,
        ViewAccess access = ViewAccess.Public,
        double sortOrder = 65535.0,
        string? logoProps = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("View name is required.", nameof(name));
        if (ownedBy == Guid.Empty)
            throw new ArgumentException("Owner user id is required.", nameof(ownedBy));

        // Auto-generate query from filters (simplified — stores raw JSON, query-time evaluation by frontend)
        var query = !string.IsNullOrWhiteSpace(filters) ? filters : null;

        return new View
        {
            Id = Guid.NewGuid(),
            Name = name,
            OwnedBy = ownedBy,
            ProjectId = projectId,
            Description = description,
            Filters = filters,
            Query = query,
            DisplayFilters = displayFilters,
            DisplayProperties = displayProperties,
            RichFilters = richFilters,
            Access = access,
            SortOrder = sortOrder,
            LogoProps = logoProps,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Updates mutable display fields. PATCH semantics — only non-null parameters are applied.
    /// </summary>
    public void Update(
        string? name = null,
        string? description = null,
        string? filters = null,
        string? displayFilters = null,
        string? displayProperties = null,
        string? richFilters = null,
        ViewAccess? access = null,
        double? sortOrder = null,
        string? logoProps = null,
        bool? isLocked = null)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name;
        if (description is not null) Description = description;
        if (filters is not null)
        {
            Filters = filters;
            Query = filters; // auto-regenerate query from filters
        }
        if (displayFilters is not null) DisplayFilters = displayFilters;
        if (displayProperties is not null) DisplayProperties = displayProperties;
        if (richFilters is not null) RichFilters = richFilters;
        if (access.HasValue) Access = access.Value;
        if (sortOrder.HasValue) SortOrder = sortOrder.Value;
        if (logoProps is not null) LogoProps = logoProps;
        if (isLocked.HasValue) IsLocked = isLocked.Value;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Archives the view. An archived view is hidden from default views
    /// but its data is preserved (inverse of <see cref="Unarchive"/>).
    /// </summary>
    public void Archive()
    {
        if (ArchivedAt is not null) return;
        ArchivedAt = DateTimeOffset.UtcNow;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Restores an archived view to active status.</summary>
    public void Unarchive()
    {
        if (ArchivedAt is null) return;
        ArchivedAt = null;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Soft-deletes this view.
    /// </summary>
    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = now;
        LastModifiedOnUtc = now;
    }

    /// <summary>Locks the view for editing.</summary>
    public void Lock()
    {
        IsLocked = true;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Unlocks the view for editing.</summary>
    public void Unlock()
    {
        IsLocked = false;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Updates the sort order value for view listing UI.</summary>
    public void SetSortOrder(double sortOrder)
    {
        SortOrder = sortOrder;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }
}