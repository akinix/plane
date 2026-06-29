using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Page.Contracts.DTOs;
using YH.Modules.Page.Contracts.v1.Pages.UpdatePageDescription;
using YH.Modules.Page.Data;
using PageEntity = YH.Modules.Page.Domain.Page;

namespace YH.Modules.Page.Features.v1.Pages.UpdatePageDescription;

/// <summary>
/// Handles <see cref="UpdatePageDescriptionCommand"/> — updates a page's description content fields.
/// </summary>
public sealed class UpdatePageDescriptionCommandHandler : ICommandHandler<UpdatePageDescriptionCommand, PageDetailDto>
{
    private readonly PageDbContext _db;

    public UpdatePageDescriptionCommandHandler(PageDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<PageDetailDto> Handle(UpdatePageDescriptionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.PageId == Guid.Empty)
        {
            throw new CustomException("Page id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var page = await _db.Pages
            .FirstOrDefaultAsync(p => p.Id == command.PageId && !p.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (page is null)
        {
            throw new NotFoundException($"Page '{command.PageId}' was not found.");
        }

        page.UpdateDescription(command.DescriptionHtml, command.DescriptionStripped, command.DescriptionJson);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return PageDtoMapper.ToDetailDto(page);
    }
}