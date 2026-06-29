using YH.Framework.Core.Domain;

namespace YH.Modules.WorkItems.Domain;

/// <summary>
/// Estimate entity — an estimate system with points or categories (Plane <c>models/estimate.py</c>).
/// Each project can have multiple estimate systems; one is marked as <see cref="IsLastUsed"/>.
/// Estimate points are child entities in the <see cref="EstimatePoints"/> collection.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <c>Estimate</c> is tenant-scoped via <see cref="IHasTenant"/>.
/// <para>
/// <b>EstimatePoint cascade:</b> When an Estimate is deleted, all its EstimatePoints are cascade-deleted
/// by EF Core (see <c>EstimateConfiguration</c>).
/// </para>
/// </remarks>
public sealed class Estimate : IHasDomainEvents, IHasTenant, ISoftDeletable, IAuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private List<EstimatePoint> _estimatePoints = [];

    public Guid Id { get; private set; }

    /// <summary>Display name for the estimate system (e.g. "Fibonacci", "T-shirt sizes").</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Estimate type: "points" (numeric) or "categories" (e.g. XS/S/M/L/XL).</summary>
    public string Type { get; private set; } = default!;

    /// <summary>Project that owns this estimate (scalar Guid, no cross-module FK).</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Whether this estimate is the project's current active estimate system.</summary>
    public bool IsLastUsed { get; private set; }

    /// <summary>Collection of estimate points (owned child entities, cascade deleted).</summary>
    public IReadOnlyList<EstimatePoint> EstimatePoints => _estimatePoints.AsReadOnly();

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

    private Estimate() { } // EF Core

    /// <summary>
    /// Factory — creates a new active <see cref="Estimate"/> system.
    /// </summary>
    /// <param name="name">Display name (non-empty, max 255).</param>
    /// <param name="type">Estimate type: "points" or "categories".</param>
    /// <param name="projectId">Project that owns this estimate.</param>
    public static Estimate Create(string name, string type, Guid projectId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Estimate name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(type) || (type != "points" && type != "categories"))
            throw new ArgumentException("Estimate type must be 'points' or 'categories'.", nameof(type));
        if (projectId == Guid.Empty)
            throw new ArgumentException("Project id is required.", nameof(projectId));

        return new Estimate
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            ProjectId = projectId,
            IsLastUsed = false,
            _estimatePoints = [],
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Updates immutable display fields.
    /// </summary>
    public void Update(string? name = null, string? type = null)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name;
        if (type is not null)
        {
            if (type != "points" && type != "categories")
                throw new ArgumentException("Estimate type must be 'points' or 'categories'.", nameof(type));
            Type = type;
        }
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Adds an estimate point to this estimate system.</summary>
    public void AddPoint(EstimatePoint point)
    {
        ArgumentNullException.ThrowIfNull(point);
        _estimatePoints.Add(point);
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Removes an estimate point by its id.</summary>
    public void RemovePoint(Guid pointId)
    {
        var point = _estimatePoints.Find(ep => ep.Id == pointId);
        if (point is not null)
        {
            _estimatePoints.Remove(point);
            LastModifiedOnUtc = DateTimeOffset.UtcNow;
        }
    }

    /// <summary>Marks this estimate as the project's last-used estimate system.</summary>
    public void MarkAsLastUsed()
    {
        if (IsLastUsed) return;
        IsLastUsed = true;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Soft-deletes this estimate.</summary>
    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = now;
    }
}
