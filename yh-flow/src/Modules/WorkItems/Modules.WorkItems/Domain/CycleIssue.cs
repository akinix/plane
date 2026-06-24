using YH.Framework.Core.Domain;

namespace YH.Modules.WorkItems.Domain;

/// <summary>
/// Cycle-Issue M2M through entity (Plane <c>models/cycle.py</c> CycleIssue).
/// Soft-deletable to support removing issues from cycles without data loss.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <c>CycleIssue</c> is tenant-scoped via <see cref="IHasTenant"/>.
/// <para>
/// <b>Unique constraint:</b> A unique index on <c>(TenantId, CycleId, IssueId)</c> with
/// <c>HasFilter("[DeletedOnUtc] IS NULL")</c> prevents duplicate issue membership in a cycle.
/// </para>
/// </remarks>
public sealed class CycleIssue : IHasTenant, ISoftDeletable
{
    public Guid Id { get; private set; }

    /// <summary>Issue id (FK, cascade delete).</summary>
    public Guid IssueId { get; private set; }

    /// <summary>Cycle id (FK, cascade delete).</summary>
    public Guid CycleId { get; private set; }

    /// <summary>When this issue was added to the cycle.</summary>
    public DateTimeOffset CreatedOnUtc { get; private set; }

    // IHasTenant — populated by Finbuckle on save.
    public string TenantId { get; private set; } = default!;

    // ISoftDeletable.
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    private CycleIssue() { } // EF Core

    /// <summary>
    /// Factory — creates a new <see cref="CycleIssue"/> association.
    /// </summary>
    public static CycleIssue Create(Guid issueId, Guid cycleId)
    {
        if (issueId == Guid.Empty)
            throw new ArgumentException("Issue id is required.", nameof(issueId));
        if (cycleId == Guid.Empty)
            throw new ArgumentException("Cycle id is required.", nameof(cycleId));

        return new CycleIssue
        {
            Id = Guid.NewGuid(),
            IssueId = issueId,
            CycleId = cycleId,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>Soft-deletes this cycle-issue association (idempotent).</summary>
    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = now;
    }
}
