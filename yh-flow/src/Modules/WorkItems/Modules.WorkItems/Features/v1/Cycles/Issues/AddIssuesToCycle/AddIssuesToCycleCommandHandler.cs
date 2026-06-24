using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Cycles.Issues;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Cycles.Issues.AddIssuesToCycle;

/// <summary>
/// Handles <see cref="AddIssuesToCycleCommand"/> — adds issues to a cycle.
/// Validates the cycle is not COMPLETED before adding issues.
/// </summary>
public sealed class AddIssuesToCycleCommandHandler : ICommandHandler<AddIssuesToCycleCommand, Unit>
{
    private readonly WorkItemsDbContext _db;

    public AddIssuesToCycleCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(AddIssuesToCycleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ProjectId == Guid.Empty)
        {
            throw new CustomException("Project id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }
        if (command.CycleId == Guid.Empty)
        {
            throw new CustomException("Cycle id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        // Verify cycle exists and is not COMPLETED
        var cycle = await _db.Cycles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == command.CycleId && c.ProjectId == command.ProjectId && !c.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (cycle is null)
        {
            throw new NotFoundException($"Cycle '{command.CycleId}' was not found.");
        }

        // COMPLETED cycles cannot accept new issues
        if (cycle.EndDate.HasValue && cycle.EndDate.Value < DateTimeOffset.UtcNow)
        {
            throw new CustomException("Cannot add issues to a completed cycle.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        // Add each issue to the cycle
        foreach (var issueId in command.IssueIds)
        {
            if (issueId == Guid.Empty) continue;

            var cycleIssue = CycleIssue.Create(issueId, command.CycleId);
            _db.Set<CycleIssue>().Add(cycleIssue);
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
