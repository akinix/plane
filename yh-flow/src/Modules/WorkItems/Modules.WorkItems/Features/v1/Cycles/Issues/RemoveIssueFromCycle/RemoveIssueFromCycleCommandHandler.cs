using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Cycles.Issues;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Cycles.Issues.RemoveIssueFromCycle;

/// <summary>
/// Handles <see cref="RemoveIssueFromCycleCommand"/> — removes an issue from a cycle via soft-delete.
/// </summary>
public sealed class RemoveIssueFromCycleCommandHandler : ICommandHandler<RemoveIssueFromCycleCommand, Unit>
{
    private readonly WorkItemsDbContext _db;

    public RemoveIssueFromCycleCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(RemoveIssueFromCycleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var cycleIssue = await _db.Set<Domain.CycleIssue>()
            .FirstOrDefaultAsync(ci => ci.CycleId == command.CycleId
                                    && ci.IssueId == command.IssueId
                                    && !ci.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (cycleIssue is null)
        {
            throw new NotFoundException($"CycleIssue for cycle '{command.CycleId}' and issue '{command.IssueId}' was not found.");
        }

        cycleIssue.SoftDelete(DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
