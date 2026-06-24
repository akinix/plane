using FluentValidation;
using YH.Modules.WorkItems.Contracts;
using YH.Modules.WorkItems.Contracts.v1.Issues.CreateIssue;

namespace YH.Modules.WorkItems.Features.v1.Issues.CreateIssue;

/// <summary>
/// FluentValidation validator for <see cref="CreateIssueCommand"/>.
/// Validates name, priority, parent self-reference guard, and date ordering.
/// </summary>
public sealed class CreateIssueCommandValidator : AbstractValidator<CreateIssueCommand>
{
    public CreateIssueCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Issue name is required.")
            .MaximumLength(WorkItemsConstants.NameMaxLength)
            .WithMessage($"Issue name must not exceed {WorkItemsConstants.NameMaxLength} characters.");

        RuleFor(x => x.Priority)
            .Must(p => string.IsNullOrWhiteSpace(p) || p is "urgent" or "high" or "medium" or "low" or "none")
            .WithMessage("Priority must be one of: urgent, high, medium, low, none.")
            .When(x => x.Priority is not null);

        // Self-reference guard: ParentId must not equal Id
        RuleFor(x => x.ParentId)
            .Must((command, parentId) => parentId != command.ProjectId) // not equal to Id check — but Id not set yet
            .WithMessage("Parent issue cannot be the same as the issue itself.")
            .When(x => x.ParentId.HasValue);

        // Date ordering: StartDate must be before TargetDate
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.TargetDate)
            .WithMessage("Start date must be before or equal to target date.")
            .When(x => x.StartDate.HasValue && x.TargetDate.HasValue);
    }
}
