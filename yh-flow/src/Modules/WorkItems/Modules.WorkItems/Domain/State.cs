using YH.Framework.Core.Domain;

namespace YH.Modules.WorkItems.Domain;

/// <summary>
/// State entity — a project-scoped status label within a fixed group (Plane <c>models/state.py</c>).
/// Each project has 5 default states (one per group) and can add custom states within those groups.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <c>State</c> is tenant-scoped via <see cref="IHasTenant"/>.
/// <c>BaseDbContext.OnModelCreating</c> auto-applies <c>IsMultiTenant()</c> and Finbuckle
/// injects <see cref="TenantId"/> on save. The composite unique index
/// <c>(TenantId, ProjectId, Name)</c> with <c>HasFilter("[DeletedOnUtc] IS NULL")</c> enforces
/// unique state names per project.
/// <para>
/// <b>Group semantics (CONTEXT &#xa7;灰色区域 2):</b> <see cref="StateGroup.Completed"/> and <see cref="StateGroup.Cancelled"/>
/// groups are considered "closed". Issues in closed states cannot be edited without first reopening.
/// </para>
/// </remarks>
public sealed class State : IHasDomainEvents, IHasTenant, ISoftDeletable, IAuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; private set; }

    /// <summary>Display name (e.g. "Todo", "In Progress", "Done"). Max 255 chars.</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Optional hex color code (e.g. "#F59E0B"). Max 7 chars.</summary>
    public string? Color { get; private set; }

    /// <summary>Group classification — one of 5 fixed values (Backlog/Unstarted/Started/Completed/Cancelled).</summary>
    public StateGroup Group { get; private set; }

    /// <summary>Project that owns this state (scalar Guid, no cross-module FK).</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Whether this state is the default for its group within the project.</summary>
    public bool IsDefault { get; private set; }

    /// <summary>Sort order for state listing UI. Default 65535.0 (Plane convention).</summary>
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

    private State() { } // EF Core

    /// <summary>
    /// Factory — creates a new active <see cref="State"/>.
    /// </summary>
    /// <param name="name">Display name (non-empty, max 255).</param>
    /// <param name="color">Optional hex color code (e.g. "#F59E0B").</param>
    /// <param name="group">Group classification (one of 5 fixed values).</param>
    /// <param name="projectId">Project that owns this state.</param>
    /// <param name="isDefault">Whether this is the default state for its group.</param>
    /// <param name="sortOrder">Sort order (default 65535.0).</param>
    public static State Create(
        string name,
        string? color,
        StateGroup group,
        Guid projectId,
        bool isDefault,
        double sortOrder = 65535.0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("State name is required.", nameof(name));
        if (projectId == Guid.Empty)
            throw new ArgumentException("Project id is required.", nameof(projectId));

        // Validate group is a known value
        _ = group switch
        {
            StateGroup.Backlog or StateGroup.Unstarted or StateGroup.Started
                or StateGroup.Completed or StateGroup.Cancelled => group,
            _ => throw new ArgumentException($"Invalid state group: {group}", nameof(group))
        };

        return new State
        {
            Id = Guid.NewGuid(),
            Name = name,
            Color = color,
            Group = group,
            ProjectId = projectId,
            IsDefault = isDefault,
            SortOrder = sortOrder,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Updates mutable display fields.
    /// </summary>
    public void Update(string? name = null, string? color = null, double? sortOrder = null)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name;
        if (color is not null) Color = color;
        if (sortOrder.HasValue) SortOrder = sortOrder.Value;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Soft-deletes this state.</summary>
    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = now;
    }
}
