using YH.Framework.Core.Domain;

namespace YH.Modules.WorkItems.Domain;

/// <summary>
/// Cycle entity — a time-boxed iteration for grouping issues (Plane <c>models/cycle.py</c>).
/// Implements <see cref="IHasTenant"/> for multi-tenant isolation, <see cref="ISoftDeletable"/>
/// for soft deletes, and <see cref="IAuditableEntity"/> for audit trails.
/// </summary>
/// <remarks>
/// <b>Date validation:</b> StartDate and EndDate must both be null or both be non-null.
/// A cycle without dates is a "backlog cycle" (Plane behavior).
/// <para>
/// <b>ProgressSnapshot:</b> A JSON blob stored when the cycle is archived or on demand,
/// providing a frozen view of completion stats at that point in time.
/// </para>
/// </remarks>
public sealed class Cycle : IHasTenant, ISoftDeletable, IAuditableEntity
{
    public Guid Id { get; private set; }

    /// <summary>Cycle name (max 255, required).</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Optional description (max 10000 chars).</summary>
    public string? Description { get; private set; }

    /// <summary>Cycle start date (nullable — both null means backlog cycle).</summary>
    public DateTimeOffset? StartDate { get; private set; }

    /// <summary>Cycle end date (nullable — both null means backlog cycle).</summary>
    public DateTimeOffset? EndDate { get; private set; }

    /// <summary>Drag-and-drop sort order (default 65535.0 per Plane convention).</summary>
    public double SortOrder { get; private set; } = 65535.0;

    /// <summary>External source identifier (e.g. GitHub milestone URL).</summary>
    public string? ExternalSource { get; private set; }

    /// <summary>External entity identifier.</summary>
    public string? ExternalId { get; private set; }

    /// <summary>Frozen JSON snapshot of cycle progress at archive time.</summary>
    public string? ProgressSnapshot { get; private set; }

    /// <summary>When this cycle was archived (null if active).</summary>
    public DateTimeOffset? ArchivedAt { get; private set; }

    /// <summary>Logo/image properties (JSON).</summary>
    public string? LogoProps { get; private set; }

    /// <summary>Timezone for date calculations (default "UTC").</summary>
    public string Timezone { get; private set; } = "UTC";

    /// <summary>Optimistic concurrency version (default 1).</summary>
    public int Version { get; private set; } = 1;

    /// <summary>Project that owns this cycle (scalar Guid, no cross-module FK).</summary>
    public Guid ProjectId { get; private set; }

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

    private Cycle() { } // EF Core

    /// <summary>
    /// Factory — creates a new <see cref="Cycle"/>.
    /// </summary>
    /// <param name="name">Cycle name (non-empty, max 255).</param>
    /// <param name="projectId">Project that owns this cycle.</param>
    /// <param name="startDate">Optional start date (must be paired with endDate).</param>
    /// <param name="endDate">Optional end date (must be paired with startDate).</param>
    /// <param name="description">Optional description.</param>
    /// <param name="timezone">Timezone (default "UTC").</param>
    public static Cycle Create(
        string name,
        Guid projectId,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        string? description = null,
        string? timezone = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Cycle name is required.", nameof(name));
        if (projectId == Guid.Empty)
            throw new ArgumentException("Project id is required.", nameof(projectId));

        // Date validation: both null OR both non-null
        if (startDate is null != endDate is null)
            throw new ArgumentException("StartDate and EndDate must both be null or both be provided.");

        return new Cycle
        {
            Id = Guid.NewGuid(),
            Name = name,
            ProjectId = projectId,
            StartDate = startDate,
            EndDate = endDate,
            Description = description,
            Timezone = timezone ?? "UTC",
            SortOrder = 65535.0,
            Version = 1,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Assigns a specific sort order value.
    /// </summary>
    public void AssignSortOrder(double? sortOrder)
    {
        if (sortOrder.HasValue)
            SortOrder = sortOrder.Value;

        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates mutable cycle fields with nullable param PATCH semantics.
    /// </summary>
    public void Update(
        string? name = null,
        string? description = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        string? timezone = null,
        double? sortOrder = null)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name;

        if (description is not null)
            Description = description;

        // Date validation: both null OR both non-null for updates
        if (startDate is not null || endDate is not null)
        {
            var newStart = startDate ?? StartDate;
            var newEnd = endDate ?? EndDate;
            if (newStart is null != newEnd is null)
                throw new ArgumentException("StartDate and EndDate must both be null or both be provided.");
            StartDate = newStart;
            EndDate = newEnd;
        }

        if (timezone is not null)
            Timezone = timezone;

        if (sortOrder.HasValue)
            SortOrder = sortOrder.Value;

        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates restricted fields (external source/id) that require special permissions.
    /// </summary>
    public void UpdateRestricted(string? externalSource = null, string? externalId = null)
    {
        if (externalSource is not null)
            ExternalSource = externalSource;
        if (externalId is not null)
            ExternalId = externalId;

        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Archives this cycle, freezing the progress snapshot at the current state.
    /// </summary>
    public void Archive()
    {
        if (ArchivedAt is not null) return; // idempotent
        ArchivedAt = DateTimeOffset.UtcNow;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Unarchives this cycle, restoring it to active status.
    /// </summary>
    public void Unarchive()
    {
        ArchivedAt = null;
        ProgressSnapshot = null;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Freezes the current progress snapshot.
    /// </summary>
    public void FreezeSnapshot(string snapshot)
    {
        ProgressSnapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Soft-deletes this cycle (idempotent).</summary>
    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = now;
    }
}
