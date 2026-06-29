using YH.Framework.Core.Domain;

namespace YH.Modules.WorkItems.Domain;

/// <summary>
/// IssueLink entity — combined external-link + internal-relation pattern (CONTEXT §灰色区域 1).
/// Represents a link from an Issue to either an external URL or another Issue.
/// NOT soft-deletable per Plane pattern (hard-delete).
/// Implements <see cref="IHasTenant"/> and <see cref="IAuditableEntity"/>.
/// </summary>
/// <remarks>
/// <b>Combined pattern:</b> Per CONTEXT decision, this entity merges Plane's separate
/// <c>IssueLink</c> (external URLs) and <c>IssueRelation</c> (internal Issue-to-Issue)
/// into a single entity. At least one of <see cref="RelatedIssueId"/> or <see cref="Url"/>
/// must be set (validated in the handler).
/// <para>
/// <b>No ISoftDeletable:</b> IssueLink is hard-deleted per Plane pattern.
/// </para>
/// </remarks>
public sealed class IssueLink : IHasTenant, IAuditableEntity
{
    public Guid Id { get; private set; }

    /// <summary>Parent issue id (FK, required).</summary>
    public Guid IssueId { get; private set; }

    /// <summary>Related issue id (optional — for internal Issue-to-Issue relations).</summary>
    public Guid? RelatedIssueId { get; private set; }

    /// <summary>External URL (optional — for external links). Max 2048 chars.</summary>
    public string? Url { get; private set; }

    /// <summary>Display title for the link (optional). Max 255 chars.</summary>
    public string? Title { get; private set; }

    /// <summary>Link type: RelatesTo / Duplicate / Blocks / BlockedBy.</summary>
    public LinkType LinkType { get; private set; }

    /// <summary>Optional JSON metadata (max 4000 chars).</summary>
    public string? Metadata { get; private set; }

    // IHasTenant — populated by Finbuckle on save.
    public string TenantId { get; private set; } = default!;

    // IAuditableEntity — populated by AuditableEntitySaveChangesInterceptor.
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }

    private IssueLink() { } // EF Core

    /// <summary>
    /// Factory — creates a new <see cref="IssueLink"/>.
    /// </summary>
    /// <param name="issueId">Parent issue id.</param>
    /// <param name="linkType">Type of link.</param>
    /// <param name="relatedIssueId">Related issue id (optional, for internal relations).</param>
    /// <param name="url">External URL (optional, for external links).</param>
    /// <param name="title">Display title (optional).</param>
    /// <param name="metadata">Optional JSON metadata.</param>
    public static IssueLink Create(
        Guid issueId,
        LinkType linkType,
        Guid? relatedIssueId = null,
        string? url = null,
        string? title = null,
        string? metadata = null)
    {
        if (issueId == Guid.Empty)
            throw new ArgumentException("Issue id is required.", nameof(issueId));

        // At least one of RelatedIssueId or Url must be set
        if (relatedIssueId is null && string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Either a related issue id or a URL must be provided.");

        return new IssueLink
        {
            Id = Guid.NewGuid(),
            IssueId = issueId,
            RelatedIssueId = relatedIssueId,
            Url = url,
            Title = title,
            LinkType = linkType,
            Metadata = metadata,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }
}
