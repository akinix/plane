using FluentValidation;
using YH.Modules.WorkItems.Contracts.v1.Modules.Links;

namespace YH.Modules.WorkItems.Features.v1.Modules.Links.AddModuleLink;

/// <summary>
/// FluentValidation validator for <see cref="AddModuleLinkCommand"/>.
/// </summary>
public sealed class AddModuleLinkCommandValidator : AbstractValidator<AddModuleLinkCommand>
{
    public AddModuleLinkCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Link title is required.")
            .MaximumLength(255).WithMessage("Link title must not exceed 255 characters.");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Link URL is required.")
            .MaximumLength(2048).WithMessage("Link URL must not exceed 2048 characters.");
    }
}
