using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.View.Contracts.v1.Views.DeleteView;
using YH.Modules.View.Data;
using ViewEntity = YH.Modules.View.Domain.View;

namespace YH.Modules.View.Features.v1.Views.DeleteView;

/// <summary>
/// Handles <see cref="DeleteViewCommand"/> — soft-deletes a view.
/// </summary>
public sealed class DeleteViewCommandHandler : ICommandHandler<DeleteViewCommand, bool>
{
    private readonly ViewDbContext _db;

    public DeleteViewCommandHandler(ViewDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<bool> Handle(DeleteViewCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ViewId == Guid.Empty)
        {
            throw new CustomException("View id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var view = await _db.Views
            .FirstOrDefaultAsync(v => v.Id == command.ViewId && !v.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (view is null)
        {
            throw new NotFoundException($"View '{command.ViewId}' was not found.");
        }

        view.SoftDelete(DateTimeOffset.UtcNow);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}
