using YH.Framework.Core.Domain;

namespace YH.Modules.Workspace.Domain;

/// <summary>
/// Workspace root aggregate (CONTEXT.md core value, RESEARCH §Example 5, Plane
/// <c>apps/api/plane/db/models/workspace.py:88-110</c>).
/// </summary>
/// <remarks>
/// <b>Tenant isolation (RESEARCH §Pitfall 6 / threat T-2-isolation [BLOCKING]):</b>
/// <c>Workspace</c> is <see cref="IGlobalEntity"/> — it deliberately OPTS OUT of Finbuckle's
/// <c>IsMultiTenant()</c> auto-application. A workspace row IS the tenant itself: its
/// <see cref="Id"/> becomes the <c>TenantId</c> for every <c>WorkspaceMember</c> /
/// <c>WorkspaceInvitation</c> row and every downstream tenant-scoped entity (Phase 3+ Project,
/// WorkItem, ...). If this entity were tenant-scoped the resolution chain would form a cycle
/// (resolve tenant by querying a tenant-filtered table). <c>BaseDbContext.OnModelCreating</c>
/// → <c>ApplyTenantIsolationByDefault()</c> skips any <c>IGlobalEntity</c>
/// (<c>TenantIsolationExtensions.cs:41</c>), and <c>WorkspaceConfiguration</c> additionally
/// <c>Ignore</c>s <c>TenantId</c> as a fail-fast guard. <b>Do not add <c>IHasTenant</c> to this type.</b>
/// </remarks>
public sealed class Workspace : IHasDomainEvents, IGlobalEntity, ISoftDeletable, IAuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; private set; }

    /// <summary>Display name (Plane: max 80).</summary>
    public string Name { get; private set; } = default!;

    /// <summary>
    /// URL-safe slug (Plane: max 48, format <c>[a-z0-9-]</c> per D-09). Unique among ACTIVE workspaces;
    /// <see cref="SoftDelete"/> appends <c>__{epoch}</c> to release the original slug for reuse (D-08).
    /// </summary>
    public string Slug { get; private set; } = default!;

    /// <summary>
    /// Owner user id (CONTEXT D-06 — scalar <see cref="Guid"/>, NO cross-module FK to Identity).
    /// The owner is auto-enrolled as the first Admin <see cref="WorkspaceMember"/> at create time
    /// (see <c>WorkspaceMembershipService.AddOwnerAsync</c>, plan 02-05).
    /// </summary>
    public Guid OwnerId { get; private set; }

    /// <summary>Optional logo URL / asset reference (Plane: text field).</summary>
    public string? Logo { get; private set; }

    /// <summary>Organization size bucket (Plane: max 20, e.g. "1-10", "11-50").</summary>
    public string? OrganizationSize { get; private set; }

    /// <summary>IANA timezone (Plane: default "UTC").</summary>
    public string TimeZone { get; private set; } = "UTC";

    /// <summary>Background color hex (Plane: default random; we default to "#000000" per RESEARCH §Example 5).</summary>
    public string BackgroundColor { get; private set; } = "#000000";

    // IAuditableEntity — populated by AuditableEntitySaveChangesInterceptor on SaveChangesAsync.
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }

    // ISoftDeletable — populated by SoftDelete(now); applied alongside the global soft-delete query filter.
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    private Workspace() { } // EF Core

    /// <summary>
    /// Factory — creates a new active <see cref="Workspace"/>. Owner is recorded as a scalar
    /// (D-06); callers MUST also insert the first <see cref="WorkspaceMember"/> (Admin role)
    /// inside the same SaveChanges unit of work (D-06 / CreateAdminMember factory).
    /// </summary>
    /// <param name="name">Display name (non-empty).</param>
    /// <param name="slug">Pre-validated URL-safe slug (use <c>ISlugGenerator</c>).</param>
    /// <param name="ownerUserId">Owner user id (scalar, no FK).</param>
    /// <param name="logo">Optional logo URL.</param>
    /// <param name="timeZone">Optional timezone (defaults to "UTC" when null/whitespace).</param>
    /// <param name="organizationSize">Optional org-size bucket.</param>
    /// <param name="backgroundColor">Optional hex color (defaults to "#000000").</param>
    public static Workspace Create(
        string name,
        string slug,
        Guid ownerUserId,
        string? logo = null,
        string? timeZone = null,
        string? organizationSize = null,
        string? backgroundColor = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Workspace name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Workspace slug is required.", nameof(slug));
        if (ownerUserId == Guid.Empty)
            throw new ArgumentException("Owner user id is required.", nameof(ownerUserId));

        return new Workspace
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            OwnerId = ownerUserId,
            Logo = logo,
            OrganizationSize = organizationSize,
            TimeZone = string.IsNullOrWhiteSpace(timeZone) ? "UTC" : timeZone,
            BackgroundColor = string.IsNullOrWhiteSpace(backgroundColor) ? "#000000" : backgroundColor,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Updates mutable display fields. Slug and OwnerId are NOT editable here
    /// (slug transfer is a destructive operation deferred until Phase 3+; owner transfer is plan 02-05).
    /// </summary>
    public void Update(
        string? name = null,
        string? logo = null,
        string? organizationSize = null,
        string? timeZone = null,
        string? backgroundColor = null)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name;
        if (logo is not null) Logo = logo;
        if (organizationSize is not null) OrganizationSize = organizationSize;
        if (!string.IsNullOrWhiteSpace(timeZone)) TimeZone = timeZone;
        if (!string.IsNullOrWhiteSpace(backgroundColor)) BackgroundColor = backgroundColor;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Soft-deletes this workspace, releasing its slug for reuse (CONTEXT D-08 / threat T-2-softdelete).
    /// Appends <c>__{epochSeconds}</c> to <see cref="Slug"/> so the unique index continues to hold
    /// (the original slug is now free for a new workspace). Mirrors Plane's behaviour where a deleted
    /// workspace keeps its row but its slug is suffixed to avoid blocking future registrations.
    /// </summary>
    /// <param name="now">Deletion timestamp (UTC). Caller passes a fixed <see cref="DateTimeOffset"/>
    /// so the epoch suffix and <see cref="DeletedOnUtc"/> stay consistent inside one SaveChanges unit.</param>
    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = now;
        // D-08: release the original slug. epoch seconds is stable + monotonic enough to disambiguate
        // repeated create/delete of the same base slug over time.
        Slug = $"{Slug}__{(int)now.ToUnixTimeSeconds()}";
    }
}
