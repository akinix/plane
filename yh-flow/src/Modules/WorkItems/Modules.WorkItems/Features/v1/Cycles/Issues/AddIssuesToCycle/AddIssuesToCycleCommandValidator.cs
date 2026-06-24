using FluentValidation;
using YH.Modules.WorkItems.Contracts.v1.Cycles.Issues;

namespace YH.Modules.WorkItems.Features.v1.Cycles.Issues.AddIssuesToCycle;

/// <summary>
/// FluentValidation validator for <see cref="AddIssuesToCycleCommand"/>.
/// </summary>
public sealed class AddIssuesToCycleCommandValidator : AbstractValidator<AddIssuesToCycleCommand>
{
    public AddIssuesToCycleCommandValidator()
    {
        RuleFor(x => x.IssueIds)
            .NotEmpty().WithMessage("At least one issue id is required.");
    }
}
