using System.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Workspace.Contracts.DTOs;
using YH.Modules.Workspace.Contracts.v1.Workspaces.GetWorkspace;
using YH.Modules.Workspace.Data;

namespace YH.Modules.Workspace.Features.v1.Workspaces.GetWorkspace;

/// <summary>
/// Handles <see cref="GetWorkspaceQuery"/> — fetches a single workspace by slug.
/// </summary>
/// <remarks>
/// <b>Cross-tenant query (D-01 / Pitfall 6):</b> <see cref="Workspace"/> is <c>IGlobalEntity</c>, so
/// <c>db.Workspaces</c> is NOT tenant-filtered. The handler therefore MUST explicitly add
/// <c>!w.IsDeleted</c> so a soft-deleted workspace cannot be retrieved (threat T-2-softdelete +
/// T-2-tenantleak). The slug strategy already resolved <c>{slug}</c> to a tenant, but the handler
/// re-queries by slug to materialize the full entity (the strategy only carries id+name).
/// <para>
/// Per Plane behavior, any authenticated user can GET a workspace's public metadata (the role gate
/// is enforced on mutations, not reads). The endpoint applies plain <c>.RequireAuthorization()</c>.
/// </para>
/// </remarks>
public sealed class GetWorkspaceQueryHandler : IQueryHandler<GetWorkspaceQuery, WorkspaceDto>
{
    private readonly WorkspaceDbContext _db;

    public GetWorkspaceQueryHandler(WorkspaceDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<WorkspaceDto> Handle(GetWorkspaceQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (string.IsNullOrWhiteSpace(query.Slug))
        {
            throw new CustomException("Slug is required.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        // AsNoTracking — pure read; Workspace is IGlobalEntity so NO tenant filter applies; we
        // explicitly exclude soft-deleted rows so deleted workspaces do not surface.
        var ws = await _db.Workspaces
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Slug == query.Slug && !w.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (ws is null)
        {
            throw new NotFoundException($"Workspace '{query.Slug}' was not found.");
        }

        return WorkspaceDtoMapper.ToDto(ws);
    }
}
