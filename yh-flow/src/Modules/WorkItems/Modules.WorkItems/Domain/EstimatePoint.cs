using YH.Framework.Core.Domain;

namespace YH.Modules.WorkItems.Domain;

/// <summary>
/// EstimatePoint value object — a single point in an estimate scale (Plane <c>models/estimate.py</c> EstimatePoint).
/// Represents one option in the estimate system (e.g. "1", "2", "3", "S", "M", "L").
/// NOT soft-deletable — cascade-deleted when parent Estimate is deleted.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <c>EstimatePoint</c> is tenant-scoped via <see cref="IHasTenant"/>.
/// <para>
/// <b>No ISoftDeletable:</b> EstimatePoint is a child entity of <see cref="Estimate"/>. When an estimate
/// is soft-deleted or hard-deleted, its points are cascade-deleted by EF Core configuration.
/// EstimatePoint itself does not implement ISoftDeletable.
/// </para>
/// </remarks>
public sealed class EstimatePoint : IHasTenant, IAuditableEntity
{
    public Guid Id { get; private set; }

    /// <summary>Parent estimate id (FK, cascade delete).</summary>
    public Guid EstimateId { get; private set; }

    /// <summary>Sort key for the point (0, 1, 2...).</summary>
    public int Key { get; private set; }

    /// <summary>Display value (e.g. "1", "2", "3", "5", "8" or "S", "M", "L").</summary>
    public string Value { get; private set; } = default!;

    /// <summary>Sort order for point listing UI. Default 65535.0.</summary>
    public double SortOrder { get; private set; } = 65535.0;

    /// <summary>Navigation property to parent Estimate (EF Core).</summary>
    public Estimate? Estimate { get; private set; }

    // IHasTenant — populated by Finbuckle on save.
    public string TenantId { get; private set; } = default!;

    // IAuditableEntity — populated by AuditableEntitySaveChangesInterceptor.
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }

    private EstimatePoint() { } // EF Core

    /// <summary>
    /// Factory — creates a new <see cref="EstimatePoint"/>.
    /// </summary>
    /// <param name="estimateId">Parent estimate id.</param>
    /// <param name="key">Sort key (0, 1, 2...).</param>
    /// <param name="value">Display value (e.g. "1", "2", "S").</param>
    /// <param name="sortOrder">Sort order (default 65535.0).</param>
    public static EstimatePoint Create(Guid estimateId, int key, string value, double sortOrder = 65535.0)
    {
        if (estimateId == Guid.Empty)
            throw new ArgumentException("Estimate id is required.", nameof(estimateId));
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Estimate point value is required.", nameof(value));

        return new EstimatePoint
        {
            Id = Guid.NewGuid(),
            EstimateId = estimateId,
            Key = key,
            Value = value,
            SortOrder = sortOrder,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>Updates the point value and/or key.</summary>
    public void UpdateValue(string value, int? key = null)
    {
        if (!string.IsNullOrWhiteSpace(value)) Value = value;
        if (key.HasValue) Key = key.Value;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }
}
