using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Page.Contracts.v1.Pages.ArchivePage;
using YH.Modules.Page.Data;

namespace YH.Modules.Page.Features.v1.Pages.ArchivePage;

/// <summary>
/// Handles <see cref="ArchivePageCommand"/> — archives a page by setting its ArchivedAt timestamp.
/// </summary>
public sealed class ArchivePageCommandHandler : ICommandHandler<ArchivePageCommand, bool>
{
    private readonly PageDbContext _db;

    public ArchivePageCommandHandler(PageDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<bool> Handle(ArchivePageCommand command, CancellationToken cancellationToken)
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

        page.Archive();

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}