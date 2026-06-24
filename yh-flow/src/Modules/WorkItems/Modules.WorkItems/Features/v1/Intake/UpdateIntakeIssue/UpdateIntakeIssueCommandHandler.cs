using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Intake.UpdateIntakeIssue;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;
using YH.Modules.WorkItems.Features.v1.Issues;

namespace YH.Modules.WorkItems.Features.v1.Intake.UpdateIntakeIssue;

/// <summary>
/// Handles <see cref="UpdateIntakeIssueCommand"/> — status transitions for intake issues.
/// </summary>
/// <remarks>
/// <b>Status transitions (T-4-intake-02):</b> Only Pending → Accepted/Rejected/Snoozed/Duplicate.
/// Accept calls <see cref="Issue.MarkAsAccepted"/> to convert the draft Issue to an active one.
/// <para>
/// <b>Elevated permission for Accept (T-4-intake-01):</b> Accept requires Admin/Member role —
/// enforced in the endpoint via the role requirement attribute.
/// </para>
/// </remarks>
public sealed class UpdateIntakeIssueCommandHandler : ICommandHandler<UpdateIntakeIssueCommand, IntakeIssueDto>
{
    private readonly WorkItemsDbContext _db;

    public UpdateIntakeIssueCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<IntakeIssueDto> Handle(UpdateIntakeIssueCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.IntakeIssueId == Guid.Empty)
        {
            throw new CustomException("IntakeIssue id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var intakeIssue = await _db.Set<IntakeIssue>()
            .Include(i => i.Issue)
            .FirstOrDefaultAsync(i => i.Id == command.IntakeIssueId && i.ProjectId == command.ProjectId, cancellationToken)
            .ConfigureAwait(false);

        if (intakeIssue is null)
        {
            throw new NotFoundException($"IntakeIssue '{command.IntakeIssueId}' was not found.");
        }

        var newStatus = (IntakeIssueStatus)command.Status;

        // Validate transition: only Pending can transition
        if (intakeIssue.Status != IntakeIssueStatus.Pending)
        {
            throw new CustomException(
                $"Cannot transition from status '{intakeIssue.Status}'. Only Pending intake issues can change status.",
                Array.Empty<string>(),
                System.Net.HttpStatusCode.BadRequest);
        }

        // Apply the status update
        intakeIssue.UpdateStatus(newStatus, command.SnoozedTill, command.DuplicateToIssueId);

        // If Accepting, transition the draft Issue to active
        if (newStatus == IntakeIssueStatus.Accepted)
        {
            var defaultState = await _db.States
                .Where(s => s.ProjectId == command.ProjectId && s.IsDefault && s.Group != StateGroup.Cancelled)
                .OrderBy(s => s.SortOrder)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (defaultState is null)
            {
                throw new CustomException(
                    "No default state found for project.",
                    Array.Empty<string>(),
                    System.Net.HttpStatusCode.Conflict);
            }

            intakeIssue.Issue.MarkAsAccepted(defaultState.Id);
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var dto = IntakeIssueDtoMapper.ToDto(intakeIssue);
        dto.Issue = IssueDtoMapper.ToDto(intakeIssue.Issue);

        return dto;
    }
}
