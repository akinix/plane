using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Page.Contracts.v1.Pages.DeletePage;
using YH.Modules.Page.Data;
using PageEntity = YH.Modules.Page.Domain.Page;

namespace YH.Modules.Page.Features.v1.Pages.DeletePage;

/// <summary>
/// Handles <see cref="DeletePageCommand"/> — soft-deletes a page.
/// </summary>
/// <remarks>
/// Page has no slug or identifier that needs epoch release (unlike Project D-03 pattern).
/// </remarks>
public sealed class DeletePageCommandHandler : ICommandHandler<DeletePageCommand, bool>
{
    private readonly PageDbContext _db;

    public DeletePageCommandHandler(PageDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<bool> Handle(DeletePageCommand command, CancellationToken cancellationToken)
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

        page.SoftDelete(DateTimeOffset.UtcNow);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}