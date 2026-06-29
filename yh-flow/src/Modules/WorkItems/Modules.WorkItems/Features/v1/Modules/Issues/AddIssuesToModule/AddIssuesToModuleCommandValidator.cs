using FluentValidation;
using YH.Modules.WorkItems.Contracts.v1.Modules.Issues;

namespace YH.Modules.WorkItems.Features.v1.Modules.Issues.AddIssuesToModule;

/// <summary>
/// FluentValidation validator for <see cref="AddIssuesToModuleCommand"/>.
/// </summary>
public sealed class AddIssuesToModuleCommandValidator : AbstractValidator<AddIssuesToModuleCommand>
{
    public AddIssuesToModuleCommandValidator()
    {
        RuleFor(x => x.IssueIds)
            .NotEmpty().WithMessage("At least one issue id is required.");
    }
}
