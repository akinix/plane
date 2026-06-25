using FluentValidation;
using YH.Modules.Page.Contracts.v1.Pages.UpdatePageDescription;

namespace YH.Modules.Page.Features.v1.Pages.UpdatePageDescription;

/// <summary>
/// FluentValidation validator for <see cref="UpdatePageDescriptionCommand"/>.
/// </summary>
public sealed class UpdatePageDescriptionCommandValidator : AbstractValidator<UpdatePageDescriptionCommand>
{
    public UpdatePageDescriptionCommandValidator()
    {
        RuleFor(x => x.PageId)
            .NotEmpty().WithMessage("Page id is required.");
    }
}