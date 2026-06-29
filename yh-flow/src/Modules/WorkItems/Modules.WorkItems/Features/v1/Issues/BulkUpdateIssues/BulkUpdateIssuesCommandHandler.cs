using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Issues.BulkUpdateIssues;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Issues.BulkUpdateIssues;

/// <summary>
/// Handles <see cref="BulkUpdateIssuesCommand"/> — atomically updates multiple issues.
/// </summary>
/// <remarks>
/// <b>Closed-state semantics:</b> Issues in Completed or Cancelled state groups are skipped
/// (not updated). Their ids are reported in the <see cref="BulkUpdateResultDto.Skipped"/> count.
/// <para>
/// <b>Atomicity:</b> All changes are saved in a single <see cref="WorkItemsDbContext"/>
/// call. No partial updates occur if an error is encountered mid-batch.
/// </para>
/// <para>
/// <b>Activity logging:</b> Each updated issue emits domain events
/// via <see cref="Issue.UpdateDetails"/> or <see cref="Issue.UpdateState"/> for audit logging.
/// </para>
/// </remarks>
public sealed class BulkUpdateIssuesCommandHandler : ICommandHandler<BulkUpdateIssuesCommand, BulkUpdateResultDto>
{
    private readonly WorkItemsDbContext _db;

    public BulkUpdateIssuesCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<BulkUpdateResultDto> Handle(BulkUpdateIssuesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.IssueIds is null or { Count: 0 })
        {
            throw new CustomException("At least one issue id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        // At least one update field must be non-null
        if (command.StateId is null && command.AssigneeIds is null && command.Priority is null)
        {
            throw new CustomException(
                "At least one update field (StateId, AssigneeIds, Priority) must be provided.",
                Array.Empty<string>(),
                System.Net.HttpStatusCode.BadRequest);
        }

        // Fetch all issues with their current state info
        var issues = await _db.Issues
            .Include(i => i.Assignees)
            .Where(i => command.IssueIds.Contains(i.Id) && i.ProjectId == command.ProjectId && !i.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var result = new BulkUpdateResultDto();

        // Resolve state groups for closed-state check
        // Fetch states for all issues that have StateId
        var stateIds = issues
            .Where(i => i.StateId.HasValue)
            .Select(i => i.StateId!.Value)
            .Distinct()
            .ToList();

        var states = await _db.States
            .AsNoTracking()
            .Where(s => stateIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s, cancellationToken)
            .ConfigureAwait(false);

        bool? targetIsCompleted = null;
        bool? targetIsCancelled = null;

        if (command.StateId.HasValue)
        {
            var targetState = await _db.States
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == command.StateId.Value, cancellationToken)
                .ConfigureAwait(false);

            if (targetState is null)
            {
                throw new CustomException("Target state not found.", Array.Empty<string>(), System.Net.HttpStatusCode.NotFound);
            }

            targetIsCompleted = targetState.Group == StateGroup.Completed;
            targetIsCancelled = targetState.Group == StateGroup.Cancelled;
        }

        foreach (var issue in issues)
        {
            // Closed-state check: skip issues in Completed or Cancelled state
            if (issue.StateId.HasValue
                && states.TryGetValue(issue.StateId.Value, out var currentState)
                && (currentState.Group == StateGroup.Completed || currentState.Group == StateGroup.Cancelled))
            {
                result.Skipped++;
                continue;
            }

            // Apply updates using domain methods

            // State update
            if (command.StateId.HasValue && command.StateId != issue.StateId)
            {
                issue.UpdateState(
                    stateId: command.StateId.Value,
                    isCompletedGroup: targetIsCompleted ?? false,
                    isCancelledGroup: targetIsCancelled ?? false);
            }

            // Assignee full replacement
            if (command.AssigneeIds is not null)
            {
                issue.UpdateAssigneeList(command.AssigneeIds);
            }

            // Priority update
            if (command.Priority is not null)
            {
                issue.UpdateDetails(priority: command.Priority);
            }

            result.Updated++;
        }

        result.Errors = [];
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return result;
    }
}
