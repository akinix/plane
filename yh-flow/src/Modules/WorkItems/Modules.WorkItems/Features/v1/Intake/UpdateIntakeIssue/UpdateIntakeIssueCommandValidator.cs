using FluentValidation;
using YH.Modules.WorkItems.Contracts.v1.Intake.UpdateIntakeIssue;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Intake.UpdateIntakeIssue;

/// <summary>
/// Validator for <see cref="UpdateIntakeIssueCommand"/>.
/// </summary>
public sealed class UpdateIntakeIssueCommandValidator : AbstractValidator<UpdateIntakeIssueCommand>
{
    private static readonly int[] AllowedStatuses = [-2, -1, 0, 1, 2];

    public UpdateIntakeIssueCommandValidator()
    {
        RuleFor(x => x.Status)
            .Must(s => AllowedStatuses.Contains(s))
            .WithMessage($"Status must be one of: {string.Join(", ", AllowedStatuses)} (-2 Pending, -1 Rejected, 0 Snoozed, 1 Accepted, 2 Duplicate).");

        RuleFor(x => x.SnoozedTill)
            .NotNull().When(x => (IntakeIssueStatus)x.Status == IntakeIssueStatus.Snoozed)
            .WithMessage("SnoozedTill is required when status is Snoozed.");

        RuleFor(x => x.DuplicateToIssueId)
            .NotNull().When(x => (IntakeIssueStatus)x.Status == IntakeIssueStatus.Duplicate)
            .WithMessage("DuplicateToIssueId is required when status is Duplicate.");
    }
}
