using System.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Cycles.DeleteCycle;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Cycles.DeleteCycle;

/// <summary>
/// Handles <see cref="DeleteCycleCommand"/> — soft-deletes a cycle.
/// </summary>
public sealed class DeleteCycleCommandHandler : ICommandHandler<DeleteCycleCommand>
{
    private readonly WorkItemsDbContext _db;

    public DeleteCycleCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(DeleteCycleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
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

        cycle.SoftDelete(DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
