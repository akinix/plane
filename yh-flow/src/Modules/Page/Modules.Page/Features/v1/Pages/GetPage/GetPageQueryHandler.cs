using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Page.Contracts.DTOs;
using YH.Modules.Page.Contracts.v1.Pages.GetPage;
using YH.Modules.Page.Data;
using PageEntity = YH.Modules.Page.Domain.Page;

namespace YH.Modules.Page.Features.v1.Pages.GetPage;

/// <summary>
/// Handles <see cref="GetPageQuery"/> — fetches a single page by id.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <see cref="PageEntity"/> is an <c>IGlobalEntity</c> (no TenantId column),
/// so tenant isolation relies on the route slug + projectId context and <c>[RequireWorkspaceRole]</c>
/// authorization. Access control for Private pages is enforced in the handler.
/// </remarks>
public sealed class GetPageQueryHandler : IQueryHandler<GetPageQuery, PageDetailDto>
{
    private readonly PageDbContext _db;

    public GetPageQueryHandler(PageDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<PageDetailDto> Handle(GetPageQuery query, CancellationToken cancellationToken)
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