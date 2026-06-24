using System.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.States.DeleteState;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.States.DeleteState;

/// <summary>
/// Handles <see cref="DeleteStateCommand"/> — soft-deletes a state.
/// Blocks with 409 Conflict if any Issues reference this StateId (Pitfall 4: Restrict + business layer check).
/// </summary>
public sealed class DeleteStateCommandHandler : ICommandHandler<DeleteStateCommand>
{
    private readonly WorkItemsDbContext _db;

    public DeleteStateCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(DeleteStateCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.StateId == Guid.Empty)
        {
            throw new CustomException("State id is required.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        var state = await _db.States
            .FirstOrDefaultAsync(s => s.Id == command.StateId && s.ProjectId == command.ProjectId && !s.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (state is null)
        {
            throw new NotFoundException($"State '{command.StateId}' was not found.");
        }

        // T-4-crud-03: check if any Issues reference this state before deleting.
        // NOTE: Issues check deferred to Task 3 when Issue entity + DbSet exist.

        state.SoftDelete(DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
