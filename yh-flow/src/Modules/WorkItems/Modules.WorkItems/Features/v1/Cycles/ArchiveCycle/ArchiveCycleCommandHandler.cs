using System.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Cycles.ArchiveCycle;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Cycles.ArchiveCycle;

/// <summary>
/// Handles <see cref="ArchiveCycleCommand"/> — archives a completed cycle.
/// Only cycles with EndDate in the past can be archived.
/// </summary>
public sealed class ArchiveCycleCommandHandler : ICommandHandler<ArchiveCycleCommand, Unit>
{
    private readonly WorkItemsDbContext _db;

    public ArchiveCycleCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(ArchiveCycleCommand command, CancellationToken cancellationToken)
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
            .FirstOrDefaultAsync(c => c.Id == command.CycleId && c.ProjectId == command.ProjectId && !c.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (cycle is null)
        {
            throw new NotFoundException($"Cycle '{command.CycleId}' was not found.");
        }

        // Only completed cycles can be archived
        if (!cycle.EndDate.HasValue || cycle.EndDate.Value >= DateTimeOffset.UtcNow)
        {
            throw new CustomException("Only completed cycles can be archived.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        cycle.Archive();
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
