using FluentValidation;
using YH.Modules.View.Contracts.v1.Views.CreateView;

namespace YH.Modules.View.Features.v1.Views.CreateView;

/// <summary>
/// Validates <see cref="CreateViewCommand"/>.
/// </summary>
public sealed class CreateViewCommandValidator : AbstractValidator<CreateViewCommand>
{
    public CreateViewCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.OwnedBy)
            .NotEmpty();
    }
}
