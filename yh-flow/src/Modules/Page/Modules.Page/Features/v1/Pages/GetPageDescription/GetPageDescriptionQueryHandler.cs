using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Page.Contracts.DTOs;
using YH.Modules.Page.Contracts.v1.Pages.GetPageDescription;
using YH.Modules.Page.Data;
using PageEntity = YH.Modules.Page.Domain.Page;

namespace YH.Modules.Page.Features.v1.Pages.GetPageDescription;

/// <summary>
/// Handles <see cref="GetPageDescriptionQuery"/> — fetches a page's description content.
/// </summary>
public sealed class GetPageDescriptionQueryHandler : IQueryHandler<GetPageDescriptionQuery, PageDetailDto>
{
    private readonly PageDbContext _db;

    public GetPageDescriptionQueryHandler(PageDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<PageDetailDto> Handle(GetPageDescriptionQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.PageId == Guid.Empty)
        {
            throw new CustomException("Page id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var page = await _db.Pages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.PageId && !p.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (page is null)
        {
            throw new NotFoundException($"Page '{query.PageId}' was not found.");
        }

        return PageDtoMapper.ToDetailDto(page);
    }
}