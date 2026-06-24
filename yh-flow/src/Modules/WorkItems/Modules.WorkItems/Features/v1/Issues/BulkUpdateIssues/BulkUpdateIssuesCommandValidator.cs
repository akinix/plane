using FluentValidation;
using YH.Modules.WorkItems.Contracts.v1.Issues.BulkUpdateIssues;

namespace YH.Modules.WorkItems.Features.v1.Issues.BulkUpdateIssues;

/// <summary>
/// Validator for <see cref="BulkUpdateIssuesCommand"/>.
/// </summary>
public sealed class BulkUpdateIssuesCommandValidator : AbstractValidator<BulkUpdateIssuesCommand>
{
    private static readonly string[] AllowedPriorities = ["urgent", "high", "medium", "low", "none"];

    public BulkUpdateIssuesCommandValidator()
    {
        RuleFor(x => x.IssueIds)
            .NotNull().WithMessage("IssueIds is required.")
            .NotEmpty().WithMessage("At least one issue id is required.");

        RuleFor(x => x.Priority)
            .Must(p => AllowedPriorities.Contains(p))
            .When(x => x.Priority is not null)
            .WithMessage($"Priority must be one of: {string.Join(", ", AllowedPriorities)}.");

        // At least one update field must be provided
        RuleFor(x => x)
            .Must(x => x.StateId is not null || x.AssigneeIds is not null || x.Priority is not null)
            .WithMessage("At least one update field (StateId, AssigneeIds, Priority) must be provided.");
    }
}
