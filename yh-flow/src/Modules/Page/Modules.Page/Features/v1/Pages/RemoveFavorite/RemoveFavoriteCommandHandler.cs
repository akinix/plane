using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Page.Contracts.v1.Pages.RemoveFavorite;
using YH.Modules.Page.Data;

namespace YH.Modules.Page.Features.v1.Pages.RemoveFavorite;

/// <summary>
/// Handles <see cref="RemoveFavoriteCommand"/> — removes a page from user's favorites (idempotent, soft-delete).
/// </summary>
public sealed class RemoveFavoriteCommandHandler : ICommandHandler<RemoveFavoriteCommand, bool>
{
    private readonly PageDbContext _db;

    public RemoveFavoriteCommandHandler(PageDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<bool> Handle(RemoveFavoriteCommand command, CancellationToken cancellationToken)
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

        var favorite = await _db.PageFavorites
            .FirstOrDefaultAsync(f => f.PageId == command.PageId && f.UserId == command.UserId && !f.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        // If not favorited, return true (idempotent)
        if (favorite is null)
        {
            return true;
        }

        // Soft-delete the favorite
        favorite.SoftDelete(DateTimeOffset.UtcNow);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}