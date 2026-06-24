using YH.Framework.Core.Domain;

namespace YH.Modules.WorkItems.Domain;

/// <summary>
/// Issue-Label M2M through entity (Plane <c>models/issue.py</c> IssueLabel).
/// Not soft-deletable — full replacement pattern per RESEARCH Pitfall 2.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <c>IssueLabel</c> is tenant-scoped via <see cref="IHasTenant"/>.
/// <para>
/// <b>Full replacement pattern:</b> When updating an Issue's labels, the handler deletes all
/// existing rows for that Issue and bulk-inserts new ones within the same <c>SaveChanges</c>
/// transaction (RESEARCH Pitfall 2: M2M full replacement transaction safety).
/// </para>
/// </remarks>
public sealed class IssueLabel : IHasTenant
{
    public Guid Id { get; private set; }

    /// <summary>Parent issue id (FK, cascade delete).</summary>
    public Guid IssueId { get; private set; }

    /// <summary>Label id (FK to Labels table).</summary>
    public Guid LabelId { get; private set; }

    public DateTimeOffset CreatedOnUtc { get; private set; }

    // IHasTenant — populated by Finbuckle on save.
    public string TenantId { get; private set; } = default!;

    private IssueLabel() { } // EF Core

    /// <summary>
    /// Factory — creates a new <see cref="IssueLabel"/>.
    /// </summary>
    public static IssueLabel Create(Guid issueId, Guid labelId)
    {
        if (issueId == Guid.Empty)
            throw new ArgumentException("Issue id is required.", nameof(issueId));
        if (labelId == Guid.Empty)
            throw new ArgumentException("Label id is required.", nameof(labelId));

        return new IssueLabel
        {
            Id = Guid.NewGuid(),
            IssueId = issueId,
            LabelId = labelId,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }
}
