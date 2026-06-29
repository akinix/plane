using FluentValidation;
using YH.Modules.View.Contracts.v1.Views.UpdateView;

namespace YH.Modules.View.Features.v1.Views.UpdateView;

/// <summary>
/// Validates <see cref="UpdateViewCommand"/>.
/// </summary>
public sealed class UpdateViewCommandValidator : AbstractValidator<UpdateViewCommand>
{
    public UpdateViewCommandValidator()
    {
        RuleFor(x => x.ViewId)
            .NotEmpty();

        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(255);
        });
    }
}
