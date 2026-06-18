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
        // AsNoTracking for read-only path.
        var workspaceIds = await _db.Members
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
