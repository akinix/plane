using YH.Framework.Core.Domain;

namespace YH.Modules.Page.Domain;

/// <summary>
/// Bridge entity linking a Page to a Project (M2M through table).
/// Tenant-scoped via IHasTenant — tenant isolation applied by Finbuckle.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> this entity is NOT <see cref="IGlobalEntity"/>; <c>BaseDbContext</c>
/// auto-applies <c>IsMultiTenant()</c>, so Finbuckle injects the <see cref="TenantId"/> property
/// (set to the resolved workspace's <c>Id</c>). The composite unique index
/// <c>(TenantId, ProjectId, PageId)</c> enforces "one page association per project".
/// </remarks>
public sealed class ProjectPage : IHasTenant, ISoftDeletable
{
    public Guid Id { get; private set; }

    /// <summary>Page id this association references.</summary>
    public Guid PageId { get; private set; }

    /// <summary>Project id this association references.</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Finbuckle-managed tenant id (workspace Guid as string).</summary>
    public string TenantId { get; private set; } = default!;

    // ISoftDeletable.
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    private ProjectPage() { } // EF Core

    /// <summary>
    /// Factory — creates a new ProjectPage association.
    /// </summary>
    /// <param name="pageId">Page id (non-empty).</param>
    /// <param name="projectId">Project id (non-empty).</param>
    public static ProjectPage Create(Guid pageId, Guid projectId)
    {
        if (pageId == Guid.Empty)
            throw new ArgumentException("Page id is required.", nameof(pageId));
        if (projectId == Guid.Empty)
            throw new ArgumentException("Project id is required.", nameof(projectId));

        return new ProjectPage
        {
            Id = Guid.NewGuid(),
            PageId = pageId,
            ProjectId = projectId,
        };
    }

    /// <summary>Soft-deletes this association.</summary>
    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = now;
    }
}