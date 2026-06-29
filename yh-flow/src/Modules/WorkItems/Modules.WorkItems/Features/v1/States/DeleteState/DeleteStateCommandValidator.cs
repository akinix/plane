using FluentValidation;
using YH.Modules.WorkItems.Contracts.v1.States.DeleteState;

namespace YH.Modules.WorkItems.Features.v1.States.DeleteState;

/// <summary>
/// FluentValidation validator for <see cref="DeleteStateCommand"/>.
/// </summary>
public sealed class DeleteStateCommandValidator : AbstractValidator<DeleteStateCommand>
{
    public DeleteStateCommandValidator()
    {
        RuleFor(x => x.StateId)
            .NotEmpty().WithMessage("State id is required.");
    }
}
