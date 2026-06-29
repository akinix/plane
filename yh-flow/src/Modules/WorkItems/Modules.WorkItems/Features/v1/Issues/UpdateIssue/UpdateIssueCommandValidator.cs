using FluentValidation;
using YH.Modules.WorkItems.Contracts;
using YH.Modules.WorkItems.Contracts.v1.Issues.UpdateIssue;

namespace YH.Modules.WorkItems.Features.v1.Issues.UpdateIssue;

/// <summary>
/// FluentValidation validator for <see cref="UpdateIssueCommand"/>.
/// Same rules as CreateIssue validator for mutable fields.
/// </summary>
public sealed class UpdateIssueCommandValidator : AbstractValidator<UpdateIssueCommand>
{
    public UpdateIssueCommandValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(WorkItemsConstants.NameMaxLength)
            .WithMessage($"Issue name must not exceed {WorkItemsConstants.NameMaxLength} characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.Priority)
            .Must(p => p is "urgent" or "high" or "medium" or "low" or "none")
            .WithMessage("Priority must be one of: urgent, high, medium, low, none.")
            .When(x => x.Priority is not null);

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.TargetDate)
            .WithMessage("Start date must be before or equal to target date.")
            .When(x => x.StartDate.HasValue && x.TargetDate.HasValue);
    }
}
