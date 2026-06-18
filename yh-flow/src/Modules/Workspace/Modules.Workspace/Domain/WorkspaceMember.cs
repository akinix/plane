using YH.Framework.Core.Domain;
using YH.Modules.Identity.Contracts.Services;

namespace YH.Modules.Workspace.Domain;

/// <summary>
/// Membership row linking a user to a workspace (CONTEXT D-04 / D-06 / D-11).
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> this entity is NOT <see cref="IGlobalEntity"/>; <c>BaseDbContext</c>
/// auto-applies <c>IsMultiTenant()</c>, so Finbuckle injects the <see cref="TenantId"/> shadow
/// property (set to the resolved workspace's <c>Id</c> by the Finbuckle resolver pipeline).
/// <see cref="TenantId"/> here is the CLR-visible property populated by Finbuckle on
/// materialization / save — see <c>IHasTenant.cs</c>. The composite unique index
/// <c>(TenantId, UserId)</c> enforces "one membership per user per workspace" (D-04).
/// <para>
/// <b>Cross-module userId (D-04):</b> <see cref="UserId"/> is a scalar <c>string</c> (matches
/// Identity's user-id column type) with NO foreign key — referential integrity is enforced at
/// the application layer via <c>IUserIdentityService</c> batch resolution. This avoids a
/// cross-module EF Core FK that would couple the Workspace module to Identity's internal schema.
/// </para>
/// <para>
/// <b>Role (D-11):</b> persisted as <c>int</c> mirroring Plane's
/// <c>ROLE_CHOICES</c> (Admin=20, Member=15, Guest=5). The application layer converts to/from
/// <see cref="YH.Modules.Workspace.Contracts.WorkspaceRole"/> at the boundary (handlers / DTO mapping).
/// </para>
/// </remarks>
public sealed class WorkspaceMember : IHasDomainEvents, IHasTenant, ISoftDeletable, IAuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; private set; }

    /// <summary>
    /// Workspace id this membership belongs to. Populated by Finbuckle from the resolved tenant
    /// on save; deliberately ALSO stored as an explicit scalar so cross-workspace admin queries
    /// (e.g. "list every workspace a user belongs to") can read it without disabling the tenant
    /// filter. This duplicates the Finbuckle <c>TenantId</c> for a workspace-guid tenant.
    /// </summary>
    public Guid WorkspaceId { get; private set; }

    /// <summary>
    /// User id (scalar string, D-04). <see cref="IUserIdentityService"/> resolves this to
    /// <c>UserSummary</c> in batch at query time. No FK to Identity.
    /// </summary>
    public string UserId { get; private set; } = default!;

    /// <summary>
    /// Finbuckle-managed tenant id. On save Finbuckle writes the resolved tenant (workspace Guid
    /// as string) here; the <c>WorkspaceMemberConfiguration</c> composite unique index
    /// <c>(TenantId, UserId)</c> uses this column.
    /// </summary>
    public string TenantId { get; private set; } = default!;

    /// <summary>
    /// Role code (int, D-11). Values mirror <see cref="YH.Modules.Workspace.Contracts.WorkspaceRole"/>:
    /// Guest=5, Member=15, Admin=20. Stored as <c>int</c> via <c>HasConversion&lt;int&gt;()</c>.
    /// </summary>
    public int Role { get; private set; }

    /// <summary>
    /// Soft-deactivation flag (separate from <see cref="ISoftDeletable.IsDeleted"/>). Inactive
    /// members keep their row for audit but lose access (middleware filters on
    /// <c>IsActive == true</c>). Plane semantics.
    /// </summary>
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

    private WorkspaceMember() { } // EF Core

    /// <summary>
    /// Factory — creates a new active membership row. <paramref name="workspaceId"/> is captured
    /// on the entity; the Finbuckle <c>TenantId</c> is set automatically on save (workspace-scoped
    /// DbContext resolves tenant = workspace Guid before SaveChangesAsync).
    /// </summary>
    /// <param name="workspaceId">Workspace id (also the Finbuckle tenant id for this row).</param>
    /// <param name="userId">User id (scalar string per D-04).</param>
    /// <param name="role">Role code (Guest=5 / Member=15 / Admin=20).</param>
    /// <param name="isActive">Initial active state (default true).</param>
    public static WorkspaceMember Create(Guid workspaceId, string userId, int role, bool isActive = true)
    {
        if (workspaceId == Guid.Empty)
            throw new ArgumentException("Workspace id is required.", nameof(workspaceId));
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User id is required.", nameof(userId));

        return new WorkspaceMember
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
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
