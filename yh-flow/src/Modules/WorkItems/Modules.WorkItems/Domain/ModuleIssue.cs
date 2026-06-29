using YH.Framework.Core.Domain;

namespace YH.Modules.WorkItems.Domain;

/// <summary>
/// Module-Issue M2M through entity (Plane <c>models/module.py</c> ModuleIssue).
/// Soft-deletable to support removing issues from modules without data loss.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <c>ModuleIssue</c> is tenant-scoped via <see cref="IHasTenant"/>.
/// <para>
/// <b>Unique constraint:</b> A unique index on <c>(TenantId, ModuleId, IssueId)</c> with
/// <c>HasFilter("[DeletedOnUtc] IS NULL")</c> prevents duplicate issue membership in a module.
/// </para>
/// </remarks>
public sealed class ModuleIssue : IHasTenant, ISoftDeletable
{
    public Guid Id { get; private set; }

    /// <summary>Issue id (FK, cascade delete).</summary>
    public Guid IssueId { get; private set; }

    /// <summary>Module id (FK, cascade delete).</summary>
    public Guid ModuleId { get; private set; }

    /// <summary>When this issue was added to the module.</summary>
    public DateTimeOffset CreatedOnUtc { get; private set; }

    // IHasTenant — populated by Finbuckle on save.
    public string TenantId { get; private set; } = default!;

    // ISoftDeletable.
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    private ModuleIssue() { } // EF Core

    /// <summary>
    /// Factory — creates a new <see cref="ModuleIssue"/> association.
    /// </summary>
    public static ModuleIssue Create(Guid issueId, Guid moduleId)
    {
        if (issueId == Guid.Empty)
            throw new ArgumentException("Issue id is required.", nameof(issueId));
        if (moduleId == Guid.Empty)
            throw new ArgumentException("Module id is required.", nameof(moduleId));

        return new ModuleIssue
        {
            Id = Guid.NewGuid(),
            IssueId = issueId,
            ModuleId = moduleId,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>Soft-deletes this module-issue association (idempotent).</summary>
    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = now;
    }
}
