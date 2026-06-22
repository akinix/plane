using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Shared.Persistence;
using YH.Modules.Workspace.Contracts.DTOs;
using YH.Modules.Workspace.Contracts.v1.Workspaces.ListUserWorkspaces;
using YH.Modules.Workspace.Data;

namespace YH.Modules.Workspace.Features.v1.Workspaces.ListUserWorkspaces;

/// <summary>
/// Handles <see cref="ListUserWorkspacesQuery"/> — lists workspaces the current user is a member of
/// (REQ-2.1), in Plane-compatible paginated format.
/// </summary>
/// <remarks>
/// <para><b>Resolution path (D-04 + D-06):</b> memberships live in the <c>WorkspaceMembers</c>
/// table with the cross-workspace scalar <c>WorkspaceId</c> + <c>UserId</c>. We project
/// <c>WorkspaceId</c> values for the user from <c>db.Members</c>, then materialize the matching
/// <c>Workspace</c> rows. <c>Workspace</c> is <c>IGlobalEntity</c> (no tenant filter); we explicitly
/// exclude soft-deleted workspaces so users do not see deleted entries.</para>
/// <para><b>Tenant filter bypass (CR-02 — <c>02-REVIEW.md §CR-02</c>):</b> the
/// <c>db.Members</c> query MUST call <c>IgnoreQueryFilters()</c>. The top-level endpoint
/// (<c>GET /api/v1/users/me/workspaces/</c>) has no <c>{slug}</c>; the DbContext defaults to the
/// caller's current/last-resolved tenant, so Finbuckle's auto-applied <c>TenantId</c> filter would
/// silently drop every membership row whose <c>TenantId</c> != that single tenant. The whole point
/// of this endpoint is cross-workspace aggregation (REQ-2.1) — disabling the filter is the
/// correct behaviour, NOT a leak. <c>Workspace</c> below is <c>IGlobalEntity</c> and is not
/// affected.</para>
/// <para><b>N+1 avoidance (T-2-n1-list):</b> two queries total — one for the user's workspace ids,
/// one for the workspaces themselves. No loop. <c>AsNoTracking</c> throughout for read-only speed.</para>
/// <para><b>Page size cap:</b> defaults to 20, capped at 100 to bound a single request's load.</para>
/// </remarks>
public sealed class ListUserWorkspacesQueryHandler : IQueryHandler<ListUserWorkspacesQuery, PlanePagedResult<WorkspaceDto>>
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly WorkspaceDbContext _db;

    public ListUserWorkspacesQueryHandler(WorkspaceDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<PlanePagedResult<WorkspaceDto>> Handle(ListUserWorkspacesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (string.IsNullOrWhiteSpace(query.UserId))
        {
            return new PlanePagedResult<WorkspaceDto>();
        }

        var pageNumber = query.PageNumber is null or < 1 ? 1 : query.PageNumber.Value;
        var pageSize = query.PageSize switch
        {
            null or < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => query.PageSize.Value,
        };

        // WorkspaceMember rows for the user (active only — inactive members lose access per D-11).
        // CR-02 (02-REVIEW.md): IgnoreQueryFilters is REQUIRED here. This is a top-level endpoint
        // (GET /api/v1/users/me/workspaces/); the DbContext is scoped to the caller's current/last
        // tenant, so Finbuckle's auto-applied TenantId filter would otherwise drop every membership
        // row belonging to a DIFFERENT tenant. Cross-workspace aggregation is the entire purpose of
        // the endpoint (REQ-2.1). The Where clauses on UserId/IsActive/IsDeleted below are business
        // rules and stay intact.
        var workspaceIds = await _db.Members
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(m => m.UserId == query.UserId && m.IsActive && !m.IsDeleted)
            .Select(m => m.WorkspaceId)
            .Distinct()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (workspaceIds.Count == 0)
        {
            return new PlanePagedResult<WorkspaceDto>();
        }

        // Materialize active workspaces for those ids, paged.
        var totalCount = await _db.Workspaces
            .AsNoTracking()
            .CountAsync(w => workspaceIds.Contains(w.Id) && !w.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        var items = await _db.Workspaces
            .AsNoTracking()
            .Where(w => workspaceIds.Contains(w.Id) && !w.IsDeleted)
            .OrderByDescending(w => w.CreatedOnUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new WorkspaceDto
            {
                Id = w.Id,
                Name = w.Name,
                Slug = w.Slug,
                OwnerId = w.OwnerId,
                Logo = w.Logo,
                OrganizationSize = w.OrganizationSize,
                TimeZone = w.TimeZone,
                BackgroundColor = w.BackgroundColor,
                CreatedAt = w.CreatedOnUtc,
                UpdatedAt = w.LastModifiedOnUtc,
                DeletedAt = w.DeletedOnUtc,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var paged = new PagedResponse<WorkspaceDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize),
        };

        return PlanePagedResultFactory.FromPagedResponse(paged, query.BaseUrl);
    }
}
