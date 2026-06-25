using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.View.Contracts.v1.Views.RemoveFavorite;
using YH.Modules.View.Data;

namespace YH.Modules.View.Features.v1.Views.RemoveFavorite;

/// <summary>
/// Handles <see cref="RemoveFavoriteCommand"/> — removes a view from user's favorites (idempotent, soft-delete).
/// </summary>
public sealed class RemoveFavoriteCommandHandler : ICommandHandler<RemoveFavoriteCommand, bool>
{
    private readonly ViewDbContext _db;

    public RemoveFavoriteCommandHandler(ViewDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<bool> Handle(RemoveFavoriteCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ViewId == Guid.Empty)
        {
            throw new CustomException("View id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            throw new CustomException("User id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var favorite = await _db.ViewFavorites
            .FirstOrDefaultAsync(f => f.ViewId == command.ViewId && f.UserId == command.UserId && !f.IsDeleted, cancellationToken)
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
