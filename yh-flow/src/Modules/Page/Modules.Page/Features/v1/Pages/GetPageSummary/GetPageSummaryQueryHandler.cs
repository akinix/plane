using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Page.Contracts.DTOs;
using YH.Modules.Page.Contracts.v1.Pages.GetPageSummary;
using YH.Modules.Page.Data;
using PageEntity = YH.Modules.Page.Domain.Page;

namespace YH.Modules.Page.Features.v1.Pages.GetPageSummary;

/// <summary>
/// Handles <see cref="GetPageSummaryQuery"/> — returns page counts and recently updated pages.
/// </summary>
public sealed class GetPageSummaryQueryHandler : IQueryHandler<GetPageSummaryQuery, object>
{
    private readonly PageDbContext _db;

    public GetPageSummaryQueryHandler(PageDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<object> Handle(GetPageSummaryQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var totalPages = await _db.Pages
            .CountAsync(p => !p.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        var totalArchived = await _db.Pages
            .CountAsync(p => !p.IsDeleted && p.ArchivedAt != null, cancellationToken)
            .ConfigureAwait(false);

        var recentPages = await _db.Pages
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.ArchivedAt == null)
            .OrderByDescending(p => p.LastModifiedOnUtc ?? p.CreatedOnUtc)
            .Take(5)
            .Select(p => PageDtoMapper.ToDto(p))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new
        {
            total_pages = totalPages,
            total_archived_pages = totalArchived,
            recently_updated = recentPages
        };
    }
}