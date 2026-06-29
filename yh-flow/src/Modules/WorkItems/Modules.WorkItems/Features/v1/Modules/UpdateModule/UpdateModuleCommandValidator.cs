using FluentValidation;
using YH.Modules.WorkItems.Contracts.Constants;
using YH.Modules.WorkItems.Contracts.v1.Modules.UpdateModule;

namespace YH.Modules.WorkItems.Features.v1.Modules.UpdateModule;

/// <summary>
/// FluentValidation validator for <see cref="UpdateModuleCommand"/>.
/// </summary>
public sealed class UpdateModuleCommandValidator : AbstractValidator<UpdateModuleCommand>
{
    public UpdateModuleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Module name is required.")
            .MaximumLength(ModuleConstants.NameMaxLength)
            .WithMessage($"Module name must not exceed {ModuleConstants.NameMaxLength} characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.Description)
            .MaximumLength(ModuleConstants.DescriptionMaxLength)
            .WithMessage($"Module description must not exceed {ModuleConstants.DescriptionMaxLength} characters.")
            .When(x => x.Description is not null);
    }
}
