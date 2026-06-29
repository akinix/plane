using YH.Framework.Core.Domain;
using YH.Modules.WorkItems.Contracts.Constants;

namespace YH.Modules.WorkItems.Domain;

/// <summary>
/// Module entity — a feature grouping for issues (Plane <c>models/module.py</c>).
/// Implements <see cref="IHasTenant"/> for multi-tenant isolation, <see cref="ISoftDeletable"/>
/// for soft deletes, and <see cref="IAuditableEntity"/> for audit trails.
/// </summary>
/// <remarks>
/// <b>Status:</b> String-based enum values: "backlog", "planned", "in-progress", "paused",
/// "completed", "cancelled" — default "planned".
/// <para>
/// <b>Unique constraint:</b> (TenantId, ProjectId, Name) with
/// <c>HasFilter("[DeletedOnUtc] IS NULL")</c> ensures unique module names per project.
/// </para>
/// <para>
/// <b>Date fields:</b> StartDate and TargetDate are independent (no pairing rule — unlike Cycle).
/// </para>
/// </remarks>
public sealed class Module : IHasTenant, ISoftDeletable, IAuditableEntity
{
    public Guid Id { get; private set; }

    /// <summary>Module name (max 255, required).</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Optional description (max 10000 chars).</summary>
    public string? Description { get; private set; }

    /// <summary>Module status — default "planned" (backlog/planned/in-progress/paused/completed/cancelled).</summary>
    public string Status { get; private set; } = ModuleConstants.DefaultStatus;

    /// <summary>Module start date (nullable — no pairing rule with TargetDate).</summary>
    public DateTimeOffset? StartDate { get; private set; }

    /// <summary>Module target/end date (nullable — no pairing rule with StartDate).</summary>
    public DateTimeOffset? TargetDate { get; private set; }

    /// <summary>Drag-and-drop sort order (default 65535.0 per Plane convention).</summary>
    public double SortOrder { get; private set; } = ModuleConstants.DefaultSortOrder;

    /// <summary>Module lead/owner (scalar Guid, no cross-module FK navigation).</summary>
    public Guid? LeadId { get; private set; }

    /// <summary>Frozen JSON snapshot of module progress at archive time.</summary>
    public string? ProgressSnapshot { get; private set; }

    /// <summary>When this module was archived (null if active).</summary>
    public DateTimeOffset? ArchivedAt { get; private set; }

    /// <summary>Logo/image properties (JSON).</summary>
    public string? LogoProps { get; private set; }

    /// <summary>Optimistic concurrency version (default 1).</summary>
    public int Version { get; private set; } = 1;

    /// <summary>Project that owns this module (scalar Guid, no cross-module FK).</summary>
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

    private Module() { } // EF Core

    /// <summary>
    /// Factory — creates a new <see cref="Module"/>.
    /// </summary>
    /// <param name="name">Module name (non-empty, max 255).</param>
    /// <param name="projectId">Project that owns this module.</param>
    /// <param name="status">Optional status (default "planned").</param>
    /// <param name="startDate">Optional start date (no pairing rule).</param>
    /// <param name="targetDate">Optional target date (no pairing rule).</param>
    /// <param name="description">Optional description.</param>
    /// <param name="leadId">Optional lead/owner.</param>
    public static Module Create(
        string name,
        Guid projectId,
        string? status = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? targetDate = null,
        string? description = null,
        Guid? leadId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Module name is required.", nameof(name));
        if (projectId == Guid.Empty)
            throw new ArgumentException("Project id is required.", nameof(projectId));
        if (status is not null && !ModuleConstants.IsValidStatus(status))
            throw new ArgumentException($"Invalid status '{status}'. Valid values: backlog, planned, in-progress, paused, completed, cancelled.", nameof(status));

        return new Module
        {
            Id = Guid.NewGuid(),
            Name = name,
            ProjectId = projectId,
            Status = status ?? ModuleConstants.DefaultStatus,
            StartDate = startDate,
            TargetDate = targetDate,
            Description = description,
            LeadId = leadId,
            SortOrder = ModuleConstants.DefaultSortOrder,
            Version = 1,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Updates mutable module fields with nullable param PATCH semantics.
    /// </summary>
    /// <remarks>
    /// Module has no COMPLETED edit restriction (unlike Cycle). All fields are updatable.
    /// </remarks>
    public void Update(
        string? name = null,
        string? description = null,
        string? status = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? targetDate = null,
        Guid? leadId = null,
        double? sortOrder = null)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name;

        if (description is not null)
            Description = description;

        if (status is not null)
        {
            if (!ModuleConstants.IsValidStatus(status))
                throw new ArgumentException($"Invalid status '{status}'.", nameof(status));
            Status = status;
        }

        if (startDate is not null)
            StartDate = startDate;

        if (targetDate is not null)
            TargetDate = targetDate;

        if (leadId is not null)
            LeadId = leadId;

        if (sortOrder.HasValue)
            SortOrder = sortOrder.Value;

        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates status separately (PATCH semantics, null skips).
    /// </summary>
    public void UpdateStatus(string? status)
    {
        if (status is not null)
        {
            if (!ModuleConstants.IsValidStatus(status))
                throw new ArgumentException($"Invalid status '{status}'.", nameof(status));
            Status = status;
        }

        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Archives this module.
    /// </summary>
    public void Archive()
    {
        if (ArchivedAt is not null) return; // idempotent
        ArchivedAt = DateTimeOffset.UtcNow;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Unarchives this module, restoring it to active status.
    /// </summary>
    public void Unarchive()
    {
        ArchivedAt = null;
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

    /// <summary>Soft-deletes this module (idempotent).</summary>
    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = now;
    }
}
