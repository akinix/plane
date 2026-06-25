using FluentValidation;
using YH.Modules.Page.Contracts.v1.Pages.UpdatePage;

namespace YH.Modules.Page.Features.v1.Pages.UpdatePage;

/// <summary>
/// FluentValidation validator for <see cref="UpdatePageCommand"/>.
/// </summary>
public sealed class UpdatePageCommandValidator : AbstractValidator<UpdatePageCommand>
{
    public UpdatePageCommandValidator()
    {
        RuleFor(x => x.PageId)
            .NotEmpty().WithMessage("Page id is required.");

        When(x => x.Name != null, () =>
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Page name must not be empty when provided.")
                .MaximumLength(255).WithMessage("Page name must not exceed 255 characters.");
        });
    }
}