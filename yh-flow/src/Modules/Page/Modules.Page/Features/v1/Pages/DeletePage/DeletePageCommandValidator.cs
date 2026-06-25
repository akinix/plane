using FluentValidation;
using YH.Modules.Page.Contracts.v1.Pages.DeletePage;

namespace YH.Modules.Page.Features.v1.Pages.DeletePage;

/// <summary>
/// FluentValidation validator for <see cref="DeletePageCommand"/>.
/// </summary>
public sealed class DeletePageCommandValidator : AbstractValidator<DeletePageCommand>
{
    public DeletePageCommandValidator()
    {
        RuleFor(x => x.PageId)
            .NotEmpty().WithMessage("Page id is required.");
    }
}