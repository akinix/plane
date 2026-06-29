using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Modules.View.Contracts.DTOs;
using YH.Modules.View.Contracts.v1.Views.ListViews;
using YH.Modules.View.Data;
using ViewEntity = YH.Modules.View.Domain.View;

namespace YH.Modules.View.Features.v1.Views.WorkspaceViewsList;

/// <summary>
/// Handles <see cref="ListViewsQuery"/> for workspace-scoped views — filters to projectId==null.
/// Delegates to <see cref="ListViews.ListViewsQueryHandler"/> logic pattern but workspace-scoped.
/// </summary>
public sealed class WorkspaceViewsListQueryHandler : IQueryHandler<ListViewsQuery, List<ViewDto>>
{
    private readonly ViewDbContext _db;

    public WorkspaceViewsListQueryHandler(ViewDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<List<ViewDto>> Handle(ListViewsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var viewQuery = _db.Views
            .AsNoTracking()
            .Where(v => !v.IsDeleted && v.ProjectId == null);

        // Sorting: Name ascending, then CreatedAt descending (Plane behavior)
        viewQuery = viewQuery
            .OrderBy(v => v.Name)
            .ThenByDescending(v => v.CreatedOnUtc);

        // Pagination
        var skip = (query.PageNumber - 1) * query.PageSize;
        viewQuery = viewQuery.Skip(skip).Take(query.PageSize);

        var views = await viewQuery
            .Select(v => ViewDtoMapper.ToDto(v))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return views;
    }
}
