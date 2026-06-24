using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Intake.CreateIntakeIssue;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;
using YH.Modules.WorkItems.Features.v1.Issues;
using YH.Modules.WorkItems.Services;

namespace YH.Modules.WorkItems.Features.v1.Intake.CreateIntakeIssue;

/// <summary>
/// Handles <see cref="CreateIntakeIssueCommand"/> — two-step creation (RESEARCH §Pattern 9):
/// 1. Creates a draft Issue (IsDraft=true) with default state
/// 2. Creates the IntakeIssue referencing the saved Issue
/// </summary>
public sealed class CreateIntakeIssueCommandHandler : ICommandHandler<CreateIntakeIssueCommand, IntakeIssueDto>
{
    private readonly WorkItemsDbContext _db;
    private readonly IIssueSequenceService _sequenceService;

    public CreateIntakeIssueCommandHandler(WorkItemsDbContext db, IIssueSequenceService sequenceService)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _sequenceService = sequenceService ?? throw new ArgumentNullException(nameof(sequenceService));
    }

    public async ValueTask<IntakeIssueDto> Handle(CreateIntakeIssueCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ProjectId == Guid.Empty)
        {
            throw new CustomException("Project id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        // Step 1: Resolve default state
        var defaultState = await _db.States
            .Where(s => s.ProjectId == command.ProjectId && s.IsDefault)
            .OrderBy(s => s.SortOrder)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        // Step 2: Create draft Issue with IsDraft=true
        var issue = Issue.Create(
            name: command.Name,
            projectId: command.ProjectId,
            stateId: defaultState?.Id,
            descriptionHtml: command.DescriptionHtml,
            priority: command.Priority,
            isDraft: true);

        // Step 3: Auto-assign SequenceId
        var sequenceId = await _sequenceService.GetNextSequenceIdAsync(command.ProjectId, cancellationToken).ConfigureAwait(false);
        issue.SequenceId = sequenceId;

        _db.Issues.Add(issue);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Step 4: Create IntakeIssue referencing the saved Issue
        var intakeIssue = IntakeIssue.Create(
            issueId: issue.Id,
            projectId: command.ProjectId,
            source: command.Source);

        _db.Set<IntakeIssue>().Add(intakeIssue);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var dto = IntakeIssueDtoMapper.ToDto(intakeIssue);
        dto.Issue = IssueDtoMapper.ToDto(issue);

        return dto;
    }
}
