using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Page.Contracts.v1.Pages.ArchivePage;
using YH.Modules.Page.Data;

namespace YH.Modules.Page.Features.v1.Pages.ArchivePage;

/// <summary>
/// Handles <see cref="UnarchivePageCommand"/> — restores an archived page by clearing its ArchivedAt timestamp.
/// </summary>
public sealed class UnarchivePageCommandHandler : ICommandHandler<UnarchivePageCommand, bool>
{
    private readonly PageDbContext _db;

    public UnarchivePageCommandHandler(PageDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<bool> Handle(UnarchivePageCommand command, CancellationToken cancellationToken)
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

        page.Unarchive();

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}