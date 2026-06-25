using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Page.Contracts.v1.Pages.AddFavorite;
using YH.Modules.Page.Data;
using YH.Modules.Page.Domain;

namespace YH.Modules.Page.Features.v1.Pages.AddFavorite;

/// <summary>
/// Handles <see cref="AddFavoriteCommand"/> — adds a page to user's favorites (idempotent).
/// </summary>
public sealed class AddFavoriteCommandHandler : ICommandHandler<AddFavoriteCommand, bool>
{
    private readonly PageDbContext _db;

    public AddFavoriteCommandHandler(PageDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<bool> Handle(AddFavoriteCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.PageId == Guid.Empty)
        {
            throw new CustomException("Page id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            throw new CustomException("User id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        // Check if already favorited (idempotent)
        var alreadyFavorited = await _db.PageFavorites
            .AnyAsync(f => f.PageId == command.PageId && f.UserId == command.UserId && !f.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (alreadyFavorited)
        {
            return true;
        }

        var favorite = PageFavorite.Create(command.PageId, command.UserId);
        _db.PageFavorites.Add(favorite);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}