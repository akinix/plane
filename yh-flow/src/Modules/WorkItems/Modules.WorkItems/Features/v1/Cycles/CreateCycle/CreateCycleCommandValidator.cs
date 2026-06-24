using FluentValidation;
using YH.Modules.WorkItems.Contracts.v1.Cycles.CreateCycle;

namespace YH.Modules.WorkItems.Features.v1.Cycles.CreateCycle;

/// <summary>
/// FluentValidation validator for <see cref="CreateCycleCommand"/>.
/// </summary>
public sealed class CreateCycleCommandValidator : AbstractValidator<CreateCycleCommand>
{
    public CreateCycleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Cycle name is required.")
            .MaximumLength(255)
            .WithMessage("Cycle name must not exceed 255 characters.");

        // Date validation: both null OR both non-null
        RuleFor(x => x)
            .Must(x => (x.StartDate is null && x.EndDate is null) ||
                       (x.StartDate is not null && x.EndDate is not null))
            .WithMessage("StartDate and EndDate must both be null or both be provided.");
    }
}
