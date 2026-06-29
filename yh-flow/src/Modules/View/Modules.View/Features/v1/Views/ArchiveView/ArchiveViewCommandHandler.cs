using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.View.Contracts.v1.Views.ArchiveView;
using YH.Modules.View.Data;
using ViewEntity = YH.Modules.View.Domain.View;

namespace YH.Modules.View.Features.v1.Views.ArchiveView;

/// <summary>
/// Handles <see cref="ArchiveViewCommand"/> — archives a view by setting its ArchivedAt timestamp.
/// </summary>
public sealed class ArchiveViewCommandHandler : ICommandHandler<ArchiveViewCommand, bool>
{
    private readonly ViewDbContext _db;

    public ArchiveViewCommandHandler(ViewDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<bool> Handle(ArchiveViewCommand command, CancellationToken cancellationToken)
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

        view.Archive();

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}
