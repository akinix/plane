using FluentValidation;
using YH.Modules.WorkItems.Contracts.v1.Cycles.TransferCycleIssues;

namespace YH.Modules.WorkItems.Features.v1.Cycles.TransferCycleIssues;

/// <summary>
/// FluentValidation validator for <see cref="TransferCycleIssuesCommand"/>.
/// </summary>
public sealed class TransferCycleIssuesCommandValidator : AbstractValidator<TransferCycleIssuesCommand>
{
    public TransferCycleIssuesCommandValidator()
    {
        RuleFor(x => x.NewCycleId)
            .NotEmpty().WithMessage("Target cycle id is required.")
            .NotEqual(x => x.CycleId).WithMessage("Target cycle must be different from source cycle.");
    }
}
