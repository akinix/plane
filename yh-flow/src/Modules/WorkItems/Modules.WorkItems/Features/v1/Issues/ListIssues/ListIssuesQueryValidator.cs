using FluentValidation;
using YH.Modules.WorkItems.Contracts.v1.Issues.ListIssues;

namespace YH.Modules.WorkItems.Features.v1.Issues.ListIssues;

/// <summary>
/// FluentValidation validator for <see cref="ListIssuesQuery"/>.
/// Validates page size max and priority filter value.
/// </summary>
public sealed class ListIssuesQueryValidator : AbstractValidator<ListIssuesQuery>
{
    private const int MaxPageSize = 100;

    public ListIssuesQueryValidator()
    {
        RuleFor(x => x.PageSize)
            .LessThanOrEqualTo(MaxPageSize)
            .WithMessage($"Page size must not exceed {MaxPageSize}.")
            .When(x => x.PageSize.HasValue);

        RuleFor(x => x.Priority)
            .Must(p => p is "urgent" or "high" or "medium" or "low" or "none")
            .WithMessage("Priority filter must be one of: urgent, high, medium, low, none.")
            .When(x => !string.IsNullOrWhiteSpace(x.Priority));
    }
}
