using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Issues.UpdateIssue;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Issues.UpdateIssue;

/// <summary>
/// Handles <see cref="UpdateIssueCommand"/> — applies PATCH updates to an issue.
/// </summary>
/// <remarks>
/// <b>CRITICAL — closed-state semantics:</b> If the current state group is <see cref="StateGroup.Completed"/>
/// or <see cref="StateGroup.Cancelled"/>, the handler rejects non-state updates with 400 "Cannot edit closed
/// issue. Reopen first." Only state changes are allowed on closed issues.
/// <para>
/// <b>CompletedAt sync:</b> When state transitions to Completed/Cancelled group, <see cref="Issue.CompletedAt"/>
/// is set. When transitioning away, it is cleared.
/// </para>
/// <para>
/// <b>M2M full replacement:</b> If assigneeIds/labelIds are provided, old records are deleted and new ones
/// inserted within the same SaveChanges (RESEARCH Pitfall 2).
/// </para>
/// </remarks>
public sealed class UpdateIssueCommandHandler : ICommandHandler<UpdateIssueCommand, IssueDto>
{
    private readonly WorkItemsDbContext _db;

    public UpdateIssueCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<IssueDto> Handle(UpdateIssueCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.IssueId == Guid.Empty)
        {
            throw new CustomException("Issue id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        // Fetch issue with current state info for closed-state check
        var issue = await _db.Issues
            .Include(i => i.Assignees)
            .Include(i => i.Labels)
            .FirstOrDefaultAsync(i => i.Id == command.IssueId && i.ProjectId == command.ProjectId && !i.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (issue is null)
        {
            throw new NotFoundException($"Issue '{command.IssueId}' was not found.");
        }

        // Closed-state semantics (T-4-issue-crud-01)
        if (issue.StateId.HasValue)
        {
            var currentState = await _db.States
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == issue.StateId.Value, cancellationToken)
                .ConfigureAwait(false);

            if (currentState is not null &&
                (currentState.Group == StateGroup.Completed || currentState.Group == StateGroup.Cancelled))
            {
                // Allow state changes only (to reopen), reject other mutations
                if (command.StateId.HasValue && command.StateId != issue.StateId)
                {
                    // State change allowed on closed issues (reopen)
                }
                else
                {
                    throw new CustomException(
                        "Cannot edit closed issue. Reopen first.",
                        Array.Empty<string>(),
                        System.Net.HttpStatusCode.BadRequest);
                }
            }
        }

        // Handle state change with CompletedAt sync (T-4-issue-crud-02)
        if (command.StateId.HasValue && command.StateId != issue.StateId)
        {
            var newState = await _db.States
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == command.StateId.Value, cancellationToken)
                .ConfigureAwait(false);

            if (newState is not null)
            {
                issue.UpdateState(
                    stateId: newState.Id,
                    isCompletedGroup: newState.Group == StateGroup.Completed,
                    isCancelledGroup: newState.Group == StateGroup.Cancelled);
            }
        }

        // Apply other mutable field updates via UpdateDetails
        issue.UpdateDetails(
            name: command.Name,
            priority: command.Priority,
            descriptionHtml: command.DescriptionHtml,
            descriptionJson: command.DescriptionJson,
            startDate: command.StartDate,
            targetDate: command.TargetDate,
            estimatePointId: command.EstimatePointId,
            isDraft: command.IsDraft);

        // M2M assignee full replacement (T-4-issue-crud-04)
        if (command.AssigneeIds is not null)
        {
            issue.UpdateAssigneeList(command.AssigneeIds);
        }

        // M2M label full replacement (T-4-issue-crud-04)
        if (command.LabelIds is not null)
        {
            issue.UpdateLabelList(command.LabelIds);
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return IssueDtoMapper.ToDto(issue);
    }
}
