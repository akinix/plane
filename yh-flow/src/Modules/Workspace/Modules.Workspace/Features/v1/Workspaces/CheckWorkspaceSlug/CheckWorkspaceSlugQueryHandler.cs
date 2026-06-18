using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Modules.Workspace.Contracts.v1.Workspaces.CheckWorkspaceSlug;
using YH.Modules.Workspace.Data;

namespace YH.Modules.Workspace.Features.v1.Workspaces.CheckWorkspaceSlug;

/// <summary>
/// Handles <see cref="CheckWorkspaceSlugQuery"/> — reports whether a slug is taken by an ACTIVE
/// workspace (REQ-2.1). Mirrors Plane <c>POST /api/v1/workspaces/slug-check/</c>.
/// </summary>
/// <remarks>
/// <b>Soft-delete semantics (D-08):</b> a soft-deleted workspace's slug has the <c>__{epoch}</c>
/// suffix, so the original slug is free. We therefore filter on <c>!IsDeleted</c> — a slug held only
/// by a deleted workspace returns <c>Exists = false</c> (available).
/// <para>
/// <b>Threat T-2-slugenum (accept):</b> per the threat model this disclosure is accepted (Plane
/// behaviour). Any authenticated user may probe slug availability; the disclosure is bounded to
/// "is this slug taken" rather than enumeration of all workspaces.
/// </para>
/// </remarks>
public sealed class CheckWorkspaceSlugQueryHandler : IQueryHandler<CheckWorkspaceSlugQuery, CheckWorkspaceSlugResponse>
{
    private readonly WorkspaceDbContext _db;

    public CheckWorkspaceSlugQueryHandler(WorkspaceDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<CheckWorkspaceSlugResponse> Handle(CheckWorkspaceSlugQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (string.IsNullOrWhiteSpace(query.Slug))
        {
            return new CheckWorkspaceSlugResponse(Exists: false);
        }

        // AsNoTracking read; Workspace is IGlobalEntity (no tenant filter); exclude soft-deleted.
        var exists = await _db.Workspaces
            .AsNoTracking()
            .AnyAsync(w => w.Slug == query.Slug && !w.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        return new CheckWorkspaceSlugResponse(exists);
    }
}
