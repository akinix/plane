using FluentValidation;
using YH.Modules.WorkItems.Contracts;
using YH.Modules.WorkItems.Contracts.v1.Intake.CreateIntakeIssue;

namespace YH.Modules.WorkItems.Features.v1.Intake.CreateIntakeIssue;

/// <summary>
/// Validator for <see cref="CreateIntakeIssueCommand"/>.
/// </summary>
public sealed class CreateIntakeIssueCommandValidator : AbstractValidator<CreateIntakeIssueCommand>
{
    private static readonly string[] AllowedPriorities = ["urgent", "high", "medium", "low", "none"];

    public CreateIntakeIssueCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(WorkItemsConstants.NameMaxLength).WithMessage($"Name must not exceed {WorkItemsConstants.NameMaxLength} characters.");

        RuleFor(x => x.Priority)
            .Must(p => AllowedPriorities.Contains(p))
            .When(x => !string.IsNullOrWhiteSpace(x.Priority))
            .WithMessage($"Priority must be one of: {string.Join(", ", AllowedPriorities)}.");
    }
}
