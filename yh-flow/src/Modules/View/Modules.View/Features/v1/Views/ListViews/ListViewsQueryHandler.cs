using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Modules.View.Contracts.DTOs;
using YH.Modules.View.Contracts.v1.Views.ListViews;
using YH.Modules.View.Data;
using ViewEntity = YH.Modules.View.Domain.View;

namespace YH.Modules.View.Features.v1.Views.ListViews;

/// <summary>
/// Handles <see cref="ListViewsQuery"/> — returns a list of views filtered by project or workspace scope.
/// </summary>
public sealed class ListViewsQueryHandler : IQueryHandler<ListViewsQuery, List<ViewDto>>
{
    private readonly ViewDbContext _db;

    public ListViewsQueryHandler(ViewDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<List<ViewDto>> Handle(ListViewsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var viewQuery = _db.Views
            .AsNoTracking()
            .Where(v => !v.IsDeleted);

        // Scope: project-level vs workspace-level
        if (query.WorkspaceScope)
        {
            viewQuery = viewQuery.Where(v => v.ProjectId == null);
        }
        else
        {
            viewQuery = viewQuery.Where(v => v.ProjectId == query.ProjectId);
        }

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
