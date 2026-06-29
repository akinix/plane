using System.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Cycles.ArchiveCycle;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Cycles.UnarchiveCycle;

/// <summary>
/// Handles <see cref="UnarchiveCycleCommand"/> — unarchives a cycle, restoring it to active status.
/// </summary>
public sealed class UnarchiveCycleCommandHandler : ICommandHandler<UnarchiveCycleCommand, Unit>
{
    private readonly WorkItemsDbContext _db;

    public UnarchiveCycleCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(UnarchiveCycleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ProjectId == Guid.Empty)
        {
            throw new CustomException("Project id is required.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }
        if (command.CycleId == Guid.Empty)
        {
            throw new CustomException("Cycle id is required.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        var cycle = await _db.Cycles
            .FirstOrDefaultAsync(c => c.Id == command.CycleId
                                   && c.ProjectId == command.ProjectId
                                   && !c.IsDeleted
                                   && c.ArchivedAt != null, cancellationToken)
            .ConfigureAwait(false);

        if (cycle is null)
        {
            throw new NotFoundException($"Archived cycle '{command.CycleId}' was not found.");
        }

        cycle.Unarchive();
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
