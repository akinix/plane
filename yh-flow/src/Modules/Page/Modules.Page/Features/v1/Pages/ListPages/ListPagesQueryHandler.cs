using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Page.Contracts.DTOs;
using YH.Modules.Page.Contracts.v1.Pages.ListPages;
using YH.Modules.Page.Data;
using PageEntity = YH.Modules.Page.Domain.Page;

namespace YH.Modules.Page.Features.v1.Pages.ListPages;

/// <summary>
/// Handles <see cref="ListPagesQuery"/> — returns a list of pages filtered by project, archive status, and parent.
/// </summary>
public sealed class ListPagesQueryHandler : IQueryHandler<ListPagesQuery, List<PageDto>>
{
    private readonly PageDbContext _db;

    public ListPagesQueryHandler(PageDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<List<PageDto>> Handle(ListPagesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var pageQuery = _db.Pages
            .AsNoTracking()
            .Where(p => p.ProjectId == query.ProjectId)
            .Where(p => !p.IsDeleted);

        // Filter: active vs archived
        if (query.IsArchived)
        {
            pageQuery = pageQuery.Where(p => p.ArchivedAt != null);
        }
        else
        {
            pageQuery = pageQuery.Where(p => p.ArchivedAt == null);
        }

        // Filter by parent: default to top-level pages only (ParentId == null).
        // If query.Parent.HasValue, filter to a specific parent.
        // If query.Parent is explicitly null (no value), still default to top-level.
        // The plan specifies: default returns top-level pages (parent == null).
        // Clients can pass include_children=true or a specific parent to override.
        if (query.Parent.HasValue)
        {
            pageQuery = pageQuery.Where(p => p.ParentId == query.Parent);
        }
        else
        {
            // Default: top-level pages only
            pageQuery = pageQuery.Where(p => p.ParentId == null);
        }

        // Sorting: SortOrder ascending, then CreatedAt descending
        pageQuery = pageQuery
            .OrderBy(p => p.SortOrder)
            .ThenByDescending(p => p.CreatedOnUtc);

        // Pagination
        var skip = (query.PageNumber - 1) * query.PageSize;
        pageQuery = pageQuery.Skip(skip).Take(query.PageSize);

        var pages = await pageQuery
            .Select(p => PageDtoMapper.ToDto(p))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return pages;
    }
}