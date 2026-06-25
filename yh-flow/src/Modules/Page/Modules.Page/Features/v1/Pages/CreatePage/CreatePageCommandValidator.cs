using FluentValidation;
using YH.Modules.Page.Contracts.v1.Pages.CreatePage;

namespace YH.Modules.Page.Features.v1.Pages.CreatePage;

/// <summary>
/// FluentValidation validator for <see cref="CreatePageCommand"/>.
/// </summary>
public sealed class CreatePageCommandValidator : AbstractValidator<CreatePageCommand>
{
    public CreatePageCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Page name is required.")
            .MaximumLength(255).WithMessage("Page name must not exceed 255 characters.");

        RuleFor(x => x.OwnedBy)
            .NotEmpty().WithMessage("Owner user id is required.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project id is required.");
    }
}