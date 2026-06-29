using YH.Framework.Core.Domain;

namespace YH.Modules.WorkItems.Domain;

/// <summary>
/// Label entity — a project-scoped tag with optional hierarchy (Plane <c>models/label.py</c>).
/// Labels are simple key-value pairs with Name + Color, supporting parent-child hierarchy via
/// self-referencing <see cref="ParentId"/>.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <c>Label</c> is tenant-scoped via <see cref="IHasTenant"/>.
/// The composite unique index <c>(TenantId, ProjectId, Name)</c> with
/// <c>HasFilter("[DeletedOnUtc] IS NULL")</c> enforces unique label names per project.
/// <para>
/// <b>Hierarchy (CONTEXT Claude's Discretion):</b> <see cref="ParentId"/> is a nullable self-referencing FK.
/// EF configuration uses <c>OnDelete(SetNull)</c> to prevent orphan issues when a parent label is deleted.
/// Depth is not restricted at the data layer (Plane behavior).
/// </para>
/// </remarks>
public sealed class Label : IHasDomainEvents, IHasTenant, ISoftDeletable, IAuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; private set; }

    /// <summary>Display name (max 255). Unique per project (non-deleted).</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Optional hex color code (e.g. "#46A758"). Max 7 chars.</summary>
    public string? Color { get; private set; }

    /// <summary>
    /// Parent label id for hierarchy support (self-referencing FK).
    /// Null when this is a top-level label.
    /// </summary>
    public Guid? ParentId { get; private set; }

    /// <summary>Project that owns this label (scalar Guid, no cross-module FK).</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Optional description (max 1000 chars).</summary>
    public string? Description { get; private set; }

    /// <summary>Sort order for label listing UI. Default 65535.0 (Plane convention).</summary>
    public double SortOrder { get; private set; } = 65535.0;

    // IHasTenant — populated by Finbuckle on save.
    public string TenantId { get; private set; } = default!;

    // IAuditableEntity — populated by AuditableEntitySaveChangesInterceptor.
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }

    // ISoftDeletable.
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    private Label() { } // EF Core

    /// <summary>
    /// Factory — creates a new active <see cref="Label"/>.
    /// </summary>
    /// <param name="name">Display name (non-empty, max 255).</param>
    /// <param name="color">Optional hex color code.</param>
    /// <param name="projectId">Project that owns this label.</param>
    /// <param name="parentId">Optional parent label id for hierarchy.</param>
    /// <param name="description">Optional description (max 1000).</param>
    /// <param name="sortOrder">Sort order (default 65535.0).</param>
    public static Label Create(
        string name,
        string? color,
        Guid projectId,
        Guid? parentId = null,
        string? description = null,
        double sortOrder = 65535.0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Label name is required.", nameof(name));
        if (projectId == Guid.Empty)
            throw new ArgumentException("Project id is required.", nameof(projectId));

        return new Label
        {
            Id = Guid.NewGuid(),
            Name = name,
            Color = color,
            ParentId = parentId,
            ProjectId = projectId,
            Description = description,
            SortOrder = sortOrder,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Updates mutable display fields.
    /// </summary>
    public void Update(
        string? name = null,
        string? color = null,
        Guid? parentId = null,
        string? description = null,
        double? sortOrder = null)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name;
        if (color is not null) Color = color;
        if (parentId is not null) ParentId = parentId;
        if (description is not null) Description = description;
        if (sortOrder.HasValue) SortOrder = sortOrder.Value;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Soft-deletes this label.</summary>
    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = now;
    }
}
