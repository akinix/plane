using YH.Framework.Core.Domain;

namespace YH.Modules.Project.Domain;

/// <summary>
/// Project aggregate — a tenant-scoped project within a workspace (Plane <c>models/project.py</c>).
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <c>Project</c> is NOT <see cref="IGlobalEntity"/> — it is tenant-scoped
/// via Finbuckle's auto <c>IsMultiTenant()</c> applied by <c>BaseDbContext.OnModelCreating</c>
/// → <c>ApplyTenantIsolationByDefault()</c>. The <see cref="TenantId"/> property is injected
/// by Finbuckle on save, equal to the resolved workspace id. The composite unique indexes
/// (Slug, Identifier) are widened to <c>(TenantId, Slug)</c> / <c>(TenantId, Identifier)</c> by
/// <c>AdjustUniqueIndexes()</c> so uniqueness is enforced per-workspace (RESEARCH Pitfall 6).
/// <para>
/// <b>Soft delete per plan D-03:</b> <see cref="SoftDelete"/> rewrites <see cref="Slug"/> with
/// <c>__{epoch}</c> to release the original slug for reuse (unconditional unique index).
/// <see cref="Identifier"/> is NOT modified — the conditional unique index with
/// <c>HasFilter("[DeletedOnUtc] IS NULL")</c> on <c>(TenantId, Identifier)</c> releases it
/// implicitly (plan T-3-domain-03 mitigation).
/// </para>
/// <para>
/// <b>Cross-module user references (D-04/D-06):</b> <see cref="OwnerId"/>, <see cref="ProjectLeadId"/>,
/// and <see cref="DefaultAssigneeId"/> are scalar <c>Guid</c> references with NO foreign key.
/// Referential integrity is enforced at the application layer via <c>IUserIdentityService</c>.
/// </para>
/// </remarks>
public sealed class Project : IHasDomainEvents, IHasTenant, ISoftDeletable, IAuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; private set; }

    /// <summary>Display name (Plane: max 255).</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Optional description (Plane: max 5000).</summary>
    public string? Description { get; private set; }

    /// <summary>Optional plain-text description (subset of Description).</summary>
    public string? DescriptionText { get; private set; }

    /// <summary>Optional HTML description (subset of Description).</summary>
    public string? DescriptionHtml { get; private set; }

    /// <summary>Visibility: <see cref="ProjectNetwork.Secret"/> (0) or <see cref="ProjectNetwork.Public"/> (2). Defaults to <see cref="ProjectNetwork.Public"/>. Stored as int.</summary>
    public ProjectNetwork Network { get; private set; }

    /// <summary>
    /// Short uppercase identifier prefix used in issue keys (e.g. "PROJ", "ENG").
    /// Unique among non-deleted projects within the same tenant. Max length 12.
    /// Conditional unique index <c>(TenantId, Identifier)</c> with
    /// <c>HasFilter("[DeletedOnUtc] IS NULL")</c> releases the value on soft delete.
    /// </summary>
    public string Identifier { get; private set; } = default!;

    /// <summary>
    /// URL-safe slug derived from name (Plane: max 100). <see cref="SoftDelete"/> rewrites
    /// with <c>__{epoch}</c> so the unconditional unique index never blocks reuse.
    /// </summary>
    public string Slug { get; private set; } = default!;

    /// <summary>Owner user id (scalar Guid, no cross-module FK per D-06).</summary>
    public Guid OwnerId { get; private set; }

    /// <summary>Optional project lead user id.</summary>
    public Guid? ProjectLeadId { get; private set; }

    /// <summary>Optional default assignee user id.</summary>
    public Guid? DefaultAssigneeId { get; private set; }

    /// <summary>Emoji icon representation.</summary>
    public string? Emoji { get; private set; }

    /// <summary>JSON icon configuration.</summary>
    public string? IconProp { get; private set; }

    /// <summary>Optional cover image URL.</summary>
    public string? CoverImageUrl { get; private set; }

    /// <summary>JSON logo configuration.</summary>
    public string? LogoProps { get; private set; }

    /// <summary>IANA timezone (default "UTC").</summary>
    public string TimeZone { get; private set; } = "UTC";

    // Feature toggles (Plane project settings)

    public bool ModuleViewEnabled { get; private set; }

    public bool CycleViewEnabled { get; private set; }

    public bool IssueViewsViewEnabled { get; private set; }

    public bool PageViewEnabled { get; private set; } = true;

    public bool IntakeViewEnabled { get; private set; }

    public bool GuestViewAllFeatures { get; private set; }

    public bool IsTimeTrackingEnabled { get; private set; }

    public bool IsIssueTypeEnabled { get; private set; }

    // Archive / close settings

    /// <summary>Days of inactivity before auto-archive.</summary>
    public int ArchiveIn { get; private set; }

    /// <summary>Days of inactivity before auto-close.</summary>
    public int CloseIn { get; private set; }

    /// <summary>Timestamp when the project was archived (null = active).</summary>
    public DateTimeOffset? ArchivedAt { get; private set; }

    /// <summary>Optional external source identifier (for Plane import/API compatibility).</summary>
    public string? ExternalSource { get; private set; }

    /// <summary>Optional external record id (for Plane import/API compatibility).</summary>
    public string? ExternalId { get; private set; }

    /// <summary>Custom sort order for project listing UI. Default 65535.0 (Plane convention).</summary>
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

    private Project() { } // EF Core

    /// <summary>
    /// Factory — creates a new active <see cref="Project"/>.
    /// </summary>
    /// <param name="name">Display name (non-empty, max 255).</param>
    /// <param name="slug">Pre-validated URL-safe slug.</param>
    /// <param name="identifier">Short identifier prefix (max 12). Auto-uppercased via <c>ToUpperInvariant()</c> per Plane convention.</param>
    /// <param name="ownerId">Owner user id (scalar Guid).</param>
    /// <param name="description">Optional description.</param>
    /// <param name="descriptionText">Optional plain-text description.</param>
    /// <param name="descriptionHtml">Optional HTML description.</param>
    /// <param name="network">Visibility (default <see cref="ProjectNetwork.Public"/> per Plane).</param>
    /// <param name="projectLeadId">Optional project lead user id.</param>
    /// <param name="defaultAssigneeId">Optional default assignee user id.</param>
    /// <param name="emoji">Optional emoji icon.</param>
    /// <param name="iconProp">Optional JSON icon config.</param>
    /// <param name="coverImageUrl">Optional cover image URL.</param>
    /// <param name="logoProps">Optional JSON logo config.</param>
    /// <param name="timeZone">Optional timezone (defaults to "UTC" when null/whitespace).</param>
    /// <param name="externalSource">Optional external source identifier.</param>
    /// <param name="externalId">Optional external record id.</param>
    /// <param name="sortOrder">Optional sort order (default 65535.0).</param>
    public static Project Create(
        string name,
        string slug,
        string identifier,
        Guid ownerId,
        string? description = null,
        string? descriptionText = null,
        string? descriptionHtml = null,
        ProjectNetwork network = ProjectNetwork.Public,
        Guid? projectLeadId = null,
        Guid? defaultAssigneeId = null,
        string? emoji = null,
        string? iconProp = null,
        string? coverImageUrl = null,
        string? logoProps = null,
        string? timeZone = null,
        string? externalSource = null,
        string? externalId = null,
        double sortOrder = 65535.0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Project slug is required.", nameof(slug));
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Project identifier is required.", nameof(identifier));
        if (ownerId == Guid.Empty)
            throw new ArgumentException("Owner user id is required.", nameof(ownerId));

        return new Project
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            Identifier = identifier.ToUpperInvariant(),
            OwnerId = ownerId,
            Description = description,
            DescriptionText = descriptionText,
            DescriptionHtml = descriptionHtml,
            Network = network,
            ProjectLeadId = projectLeadId,
            DefaultAssigneeId = defaultAssigneeId,
            Emoji = emoji,
            IconProp = iconProp,
            CoverImageUrl = coverImageUrl,
            LogoProps = logoProps,
            TimeZone = string.IsNullOrWhiteSpace(timeZone) ? "UTC" : timeZone,
            ExternalSource = externalSource,
            ExternalId = externalId,
            SortOrder = sortOrder,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Updates mutable display fields. Slug is NOT editable here
    /// (slug changes require validation against existing project slugs).
    /// </summary>
    public void Update(
        string? name = null,
        string? description = null,
        string? descriptionText = null,
        string? descriptionHtml = null,
        ProjectNetwork? network = null,
        string? identifier = null,
        string? emoji = null,
        string? iconProp = null,
        string? coverImageUrl = null,
        string? logoProps = null,
        string? timeZone = null,
        Guid? projectLeadId = null,
        Guid? defaultAssigneeId = null,
        bool? moduleViewEnabled = null,
        bool? cycleViewEnabled = null,
        bool? issueViewsViewEnabled = null,
        bool? pageViewEnabled = null,
        bool? intakeViewEnabled = null,
        bool? guestViewAllFeatures = null,
        bool? isTimeTrackingEnabled = null,
        bool? isIssueTypeEnabled = null,
        int? archiveIn = null,
        int? closeIn = null,
        string? externalSource = null,
        string? externalId = null,
        double? sortOrder = null)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name;
        if (description is not null) Description = description;
        if (descriptionText is not null) DescriptionText = descriptionText;
        if (descriptionHtml is not null) DescriptionHtml = descriptionHtml;
        if (network.HasValue) Network = network.Value;
        if (!string.IsNullOrWhiteSpace(identifier)) Identifier = identifier.ToUpperInvariant();
        if (emoji is not null) Emoji = emoji;
        if (iconProp is not null) IconProp = iconProp;
        if (coverImageUrl is not null) CoverImageUrl = coverImageUrl;
        if (logoProps is not null) LogoProps = logoProps;
        if (!string.IsNullOrWhiteSpace(timeZone)) TimeZone = timeZone;
        if (projectLeadId is not null) ProjectLeadId = projectLeadId;
        if (defaultAssigneeId is not null) DefaultAssigneeId = defaultAssigneeId;
        if (moduleViewEnabled.HasValue) ModuleViewEnabled = moduleViewEnabled.Value;
        if (cycleViewEnabled.HasValue) CycleViewEnabled = cycleViewEnabled.Value;
        if (issueViewsViewEnabled.HasValue) IssueViewsViewEnabled = issueViewsViewEnabled.Value;
        if (pageViewEnabled.HasValue) PageViewEnabled = pageViewEnabled.Value;
        if (intakeViewEnabled.HasValue) IntakeViewEnabled = intakeViewEnabled.Value;
        if (guestViewAllFeatures.HasValue) GuestViewAllFeatures = guestViewAllFeatures.Value;
        if (isTimeTrackingEnabled.HasValue) IsTimeTrackingEnabled = isTimeTrackingEnabled.Value;
        if (isIssueTypeEnabled.HasValue) IsIssueTypeEnabled = isIssueTypeEnabled.Value;
        if (archiveIn.HasValue) ArchiveIn = archiveIn.Value;
        if (closeIn.HasValue) CloseIn = closeIn.Value;
        if (externalSource is not null) ExternalSource = externalSource;
        if (externalId is not null) ExternalId = externalId;
        if (sortOrder.HasValue) SortOrder = sortOrder.Value;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Soft-deletes this project, releasing its slug for reuse.
    /// Appends <c>__{epochSeconds}</c> to <see cref="Slug"/> so the unconditional unique index
    /// continues to hold while freeing the original slug (D-03/Workspace pattern).
    /// <see cref="Identifier"/> is NOT modified — the conditional unique index with
    /// <c>HasFilter("[DeletedOnUtc] IS NULL")</c> handles release (plan T-3-domain-03).
    /// </summary>
    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = now;
        Slug = $"{Slug}__{(int)now.ToUnixTimeSeconds()}";
    }

    /// <summary>
    /// Archives the project. An archived project is hidden from default views
    /// but its data is preserved (inverse of <see cref="Unarchive"/>).
    /// </summary>
    public void Archive(DateTimeOffset now)
    {
        if (ArchivedAt is not null) return;
        ArchivedAt = now;
        LastModifiedOnUtc = now;
    }

    /// <summary>Restores an archived project to active status.</summary>
    public void Unarchive()
    {
        if (ArchivedAt is null) return;
        ArchivedAt = null;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Updates the sort order value for project listing UI.</summary>
    public void SetSortOrder(double sortOrder)
    {
        SortOrder = sortOrder;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }
}
