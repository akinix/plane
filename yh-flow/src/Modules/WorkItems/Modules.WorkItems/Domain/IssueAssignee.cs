using YH.Framework.Core.Domain;

namespace YH.Modules.WorkItems.Domain;

/// <summary>
/// Issue-Author M2M through entity (Plane <c>models/issue.py</c> IssueAssignee).
/// Not soft-deletable — full replacement pattern per RESEARCH Pitfall 2.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <c>IssueAssignee</c> is tenant-scoped via <see cref="IHasTenant"/>.
/// <para>
/// <b>Full replacement pattern:</b> When updating an Issue's assignees, the handler deletes all
/// existing rows for that Issue and bulk-inserts new ones within the same <c>SaveChanges</c>
/// transaction (RESEARCH Pitfall 2: M2M full replacement transaction safety).
/// </para>
/// </remarks>
public sealed class IssueAssignee : IHasTenant
{
    public Guid Id { get; private set; }

    /// <summary>Parent issue id (FK, cascade delete).</summary>
    public Guid IssueId { get; private set; }

    /// <summary>Assignee user id (scalar string, no cross-module FK).</summary>
    public string AssigneeId { get; private set; } = default!;

    public DateTimeOffset CreatedOnUtc { get; private set; }

    // IHasTenant — populated by Finbuckle on save.
    public string TenantId { get; private set; } = default!;

    private IssueAssignee() { } // EF Core

    /// <summary>
    /// Factory — creates a new <see cref="IssueAssignee"/>.
    /// </summary>
    public static IssueAssignee Create(Guid issueId, string assigneeId)
    {
        if (issueId == Guid.Empty)
            throw new ArgumentException("Issue id is required.", nameof(issueId));
        if (string.IsNullOrWhiteSpace(assigneeId))
            throw new ArgumentException("Assignee id is required.", nameof(assigneeId));

        return new IssueAssignee
        {
            Id = Guid.NewGuid(),
            IssueId = issueId,
            AssigneeId = assigneeId,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }
}
