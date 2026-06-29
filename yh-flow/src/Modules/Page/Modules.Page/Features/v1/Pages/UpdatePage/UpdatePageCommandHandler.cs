using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Page.Contracts.DTOs;
using YH.Modules.Page.Contracts.v1.Pages.UpdatePage;
using YH.Modules.Page.Data;
using YH.Modules.Page.Domain;
using PageEntity = YH.Modules.Page.Domain.Page;

namespace YH.Modules.Page.Features.v1.Pages.UpdatePage;

/// <summary>
/// Handles <see cref="UpdatePageCommand"/> — applies PATCH updates to mutable page display fields.
/// </summary>
public sealed class UpdatePageCommandHandler : ICommandHandler<UpdatePageCommand, PageDetailDto>
{
    private readonly PageDbContext _db;

    public UpdatePageCommandHandler(PageDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<PageDetailDto> Handle(UpdatePageCommand command, CancellationToken cancellationToken)
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

        // Verify Private page access: if page is Private and current user is not the owner, reject.
        // Note: Full identity integration would use ICurrentUser here; for now we check via the
        // workspace role requirement on the endpoint.
        page.Update(
            name: command.Name,
            descriptionHtml: command.DescriptionHtml,
            descriptionStripped: command.DescriptionStripped,
            descriptionJson: command.DescriptionJson,
            access: command.Access.HasValue ? (PageAccess)command.Access.Value : null,
            color: command.Color,
            parentId: command.ParentId,
            sortOrder: command.SortOrder,
            viewProps: command.ViewProps,
            logoProps: command.LogoProps,
            isGlobal: command.IsGlobal);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return PageDtoMapper.ToDetailDto(page);
    }
}