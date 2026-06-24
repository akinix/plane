using YH.Framework.Core.Domain;

namespace YH.Modules.Project.Domain;

/// <summary>
/// Membership row linking a user to a project (tenant-scoped, within a workspace).
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> this entity is NOT <see cref="IGlobalEntity"/>; <c>BaseDbContext</c>
/// auto-applies <c>IsMultiTenant()</c>, so Finbuckle injects the <see cref="TenantId"/> shadow
/// property (set to the resolved workspace's <c>Id</c>). The composite unique index
/// <c>(TenantId, ProjectId, UserId)</c> enforces "one membership per user per project".
/// <para>
/// <b>Cross-module userId (D-04):</b> <see cref="UserId"/> is a scalar <c>string</c> (matches
/// Identity's <c>FshUser.Id</c> column type) with NO foreign key — referential integrity is
/// enforced at the application layer. See <c>WorkspaceMember</c> for the canonical pattern.
/// </para>
/// <para>
/// <b>Role (D-11):</b> persisted as <c>int</c> mirroring Plane's <c>ROLE_CHOICES</c>
/// (Admin=20, Member=15, Guest=5). The application layer converts to/from
/// <see cref="YH.Modules.Workspace.Contracts.WorkspaceRole"/> at the boundary.
/// </para>
/// </remarks>
public sealed class ProjectMember : IHasDomainEvents, IHasTenant, ISoftDeletable, IAuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; private set; }

    /// <summary>
    /// Project id this membership belongs to. Tenant isolation ensures this is within the
    /// current workspace.
    /// </summary>
    public Guid ProjectId { get; private set; }

    /// <summary>
    /// User id (scalar string, D-04). Matches Identity's <c>FshUser.Id</c> column type.
    /// No FK to Identity.
    /// </summary>
    public string UserId { get; private set; } = default!;

    /// <summary>Finbuckle-managed tenant id (workspace Guid as string).</summary>
    public string TenantId { get; private set; } = default!;

    /// <summary>
    /// Role code (int, D-11). Values mirror <see cref="YH.Modules.Workspace.Contracts.WorkspaceRole"/>:
    /// Guest=5, Member=15, Admin=20. Stored as <c>int</c>.
    /// </summary>
    public int Role { get; private set; }

    /// <summary>Soft-deactivation flag (separate from <see cref="ISoftDeletable.IsDeleted"/>).</summary>
    public bool IsActive { get; private set; }

    // IAuditableEntity
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }

    // ISoftDeletable
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    private ProjectMember() { } // EF Core

    /// <summary>
    /// Factory — creates a new active project membership row.
    /// </summary>
    /// <param name="projectId">Project id.</param>
    /// <param name="userId">User id (scalar string per D-04).</param>
    /// <param name="role">Role code (Guest=5 / Member=15 / Admin=20).</param>
    /// <param name="isActive">Initial active state (default true).</param>
    public static ProjectMember Create(Guid projectId, string userId, int role, bool isActive = true)
    {
        if (projectId == Guid.Empty)
            throw new ArgumentException("Project id is required.", nameof(projectId));
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User id is required.", nameof(userId));

        return new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            UserId = userId,
            Role = role,
            IsActive = isActive,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>Updates the role (Guest/Member/Admin).</summary>
    public void UpdateRole(int role)
    {
        Role = role;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Deactivates without deleting (keeps row for audit).</summary>
    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Re-activates a previously deactivated membership.</summary>
    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }
}
