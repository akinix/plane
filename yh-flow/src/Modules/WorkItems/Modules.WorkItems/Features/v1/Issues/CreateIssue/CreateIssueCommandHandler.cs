using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Issues.CreateIssue;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;
using YH.Modules.WorkItems.Services;

namespace YH.Modules.WorkItems.Features.v1.Issues.CreateIssue;

/// <summary>
/// Handles <see cref="CreateIssueCommand"/> — creates a new issue with auto-assigned SequenceId,
/// default state resolution, and M2M assignee/label creation.
/// </summary>
/// <remarks>
/// <b>SequenceId:</b> Uses <see cref="IIssueSequenceService.GetNextSequenceIdAsync"/> with SERIALIZABLE
/// transaction isolation for atomic increments per project (RESEARCH Pitfall 1).
/// <para>
/// <b>Default state:</b> If <see cref="CreateIssueCommand.StateId"/> is not provided, resolves the
/// project's default state (IsDefault=true). Falls back to first state ordered by SortOrder.
/// </para>
/// <para>
/// <b>M2M assignees/labels:</b> Created with issue in the same SaveChanges transaction.
/// </para>
/// </remarks>
public sealed class CreateIssueCommandHandler : ICommandHandler<CreateIssueCommand, CreateIssueResponse>
{
    private readonly WorkItemsDbContext _db;
    private readonly IIssueSequenceService _sequenceService;

    public CreateIssueCommandHandler(WorkItemsDbContext db, IIssueSequenceService sequenceService)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _sequenceService = sequenceService ?? throw new ArgumentNullException(nameof(sequenceService));
    }

    public async ValueTask<CreateIssueResponse> Handle(CreateIssueCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ProjectId == Guid.Empty)
        {
            throw new CustomException("Project id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        // Resolve default state if not provided
        Guid? stateId = command.StateId;
        if (stateId is null)
        {
            var defaultState = await _db.States
                .Where(s => s.ProjectId == command.ProjectId && s.IsDefault)
                .OrderBy(s => s.SortOrder)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            stateId = defaultState?.Id;
        }

        // Create the issue (SequenceId will be set after generation)
        var issue = Issue.Create(
            name: command.Name,
            projectId: command.ProjectId,
            stateId: stateId,
            parentId: command.ParentId,
            estimatePointId: command.EstimatePointId,
            descriptionHtml: command.DescriptionHtml,
            descriptionJson: command.DescriptionJson,
            priority: command.Priority,
            startDate: command.StartDate,
            targetDate: command.TargetDate,
            isDraft: command.IsDraft);

        // Auto-assign SequenceId via transaction-safe service
        var sequenceId = await _sequenceService.GetNextSequenceIdAsync(command.ProjectId, cancellationToken).ConfigureAwait(false);
        issue.SequenceId = sequenceId;

        _db.Issues.Add(issue);

        // Create IssueAssignee records if provided
        if (command.AssigneeIds is { Count: > 0 })
        {
            foreach (var assigneeId in command.AssigneeIds.Distinct())
            {
                if (!string.IsNullOrWhiteSpace(assigneeId))
                {
                    _db.IssueAssignees.Add(IssueAssignee.Create(issue.Id, assigneeId));
                }
            }
        }

        // Create IssueLabel records if provided
        if (command.LabelIds is { Count: > 0 })
        {
            foreach (var labelId in command.LabelIds.Distinct())
            {
                if (labelId != Guid.Empty)
                {
                    _db.IssueLabels.Add(IssueLabel.Create(issue.Id, labelId));
                }
            }
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CreateIssueResponse(issue.Id, issue.SequenceId);
    }
}
