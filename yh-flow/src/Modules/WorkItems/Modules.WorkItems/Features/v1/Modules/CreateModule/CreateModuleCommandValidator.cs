using FluentValidation;
using YH.Modules.WorkItems.Contracts.Constants;
using YH.Modules.WorkItems.Contracts.v1.Modules.CreateModule;

namespace YH.Modules.WorkItems.Features.v1.Modules.CreateModule;

/// <summary>
/// FluentValidation validator for <see cref="CreateModuleCommand"/>.
/// </summary>
public sealed class CreateModuleCommandValidator : AbstractValidator<CreateModuleCommand>
{
    public CreateModuleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Module name is required.")
            .MaximumLength(ModuleConstants.NameMaxLength)
            .WithMessage($"Module name must not exceed {ModuleConstants.NameMaxLength} characters.");

        RuleFor(x => x.Description)
            .MaximumLength(ModuleConstants.DescriptionMaxLength)
            .WithMessage($"Module description must not exceed {ModuleConstants.DescriptionMaxLength} characters.");
    }
}
