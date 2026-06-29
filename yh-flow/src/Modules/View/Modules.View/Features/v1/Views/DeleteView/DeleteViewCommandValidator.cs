using FluentValidation;
using YH.Modules.View.Contracts.v1.Views.DeleteView;

namespace YH.Modules.View.Features.v1.Views.DeleteView;

/// <summary>
/// Validates <see cref="DeleteViewCommand"/>.
/// </summary>
public sealed class DeleteViewCommandValidator : AbstractValidator<DeleteViewCommand>
{
    public DeleteViewCommandValidator()
    {
        RuleFor(x => x.ViewId)
            .NotEmpty();
    }
}
