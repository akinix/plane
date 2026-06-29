using System.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Page.Contracts.v1.Pages.CreatePage;
using YH.Modules.Page.Data;
using YH.Modules.Page.Domain;
using PageEntity = YH.Modules.Page.Domain.Page;

namespace YH.Modules.Page.Features.v1.Pages.CreatePage;

/// <summary>
/// Handles <see cref="CreatePageCommand"/> — creates the page aggregate and auto-creates
/// a <see cref="ProjectPage"/> bridge entity.
/// </summary>
public sealed class CreatePageCommandHandler : ICommandHandler<CreatePageCommand, CreatePageResponse>
{
    private readonly PageDbContext _db;

    public CreatePageCommandHandler(PageDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<CreatePageResponse> Handle(CreatePageCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.OwnedBy == Guid.Empty)
        {
            throw new ArgumentException("Owner user id is required.", nameof(command));
        }

        var page = PageEntity.Create(
            name: command.Name,
            projectId: command.ProjectId,
            ownedBy: command.OwnedBy,
            descriptionHtml: command.DescriptionHtml,
            descriptionStripped: command.DescriptionStripped,
            descriptionJson: command.DescriptionJson,
            access: (PageAccess)command.Access,
            color: command.Color,
            parentId: command.ParentId,
            sortOrder: command.SortOrder,
            viewProps: command.ViewProps,
            logoProps: command.LogoProps,
            isGlobal: command.IsGlobal);

        _db.Pages.Add(page);

        // auto-create ProjectPage bridge entity
        var projectPage = ProjectPage.Create(page.Id, command.ProjectId);
        _db.ProjectPages.Add(projectPage);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CreatePageResponse(page.Id);
    }
}