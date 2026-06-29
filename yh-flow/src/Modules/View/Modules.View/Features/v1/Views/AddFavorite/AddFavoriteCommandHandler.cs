using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.View.Contracts.v1.Views.AddFavorite;
using YH.Modules.View.Data;
using YH.Modules.View.Domain;

namespace YH.Modules.View.Features.v1.Views.AddFavorite;

/// <summary>
/// Handles <see cref="AddFavoriteCommand"/> — adds a view to user's favorites (idempotent).
/// </summary>
public sealed class AddFavoriteCommandHandler : ICommandHandler<AddFavoriteCommand, bool>
{
    private readonly ViewDbContext _db;

    public AddFavoriteCommandHandler(ViewDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<bool> Handle(AddFavoriteCommand command, CancellationToken cancellationToken)
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

        // Check if already favorited (idempotent)
        var alreadyFavorited = await _db.ViewFavorites
            .AnyAsync(f => f.ViewId == command.ViewId && f.UserId == command.UserId && !f.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (alreadyFavorited)
        {
            return true;
        }

        var favorite = ViewFavorite.Create(command.ViewId, command.UserId);
        _db.ViewFavorites.Add(favorite);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}
