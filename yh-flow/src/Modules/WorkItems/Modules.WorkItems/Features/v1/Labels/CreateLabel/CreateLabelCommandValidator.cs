using System.Text.RegularExpressions;
using FluentValidation;
using YH.Modules.WorkItems.Contracts;
using YH.Modules.WorkItems.Contracts.v1.Labels.CreateLabel;

namespace YH.Modules.WorkItems.Features.v1.Labels.CreateLabel;

/// <summary>
/// FluentValidation validator for <see cref="CreateLabelCommand"/>.
/// </summary>
public sealed class CreateLabelCommandValidator : AbstractValidator<CreateLabelCommand>
{
    private static readonly Regex ColorRegex = new("^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

    public CreateLabelCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Label name is required.")
            .MaximumLength(WorkItemsConstants.NameMaxLength)
            .WithMessage($"Label name must not exceed {WorkItemsConstants.NameMaxLength} characters.");

        RuleFor(x => x.Color)
            .MaximumLength(WorkItemsConstants.ColorMaxLength)
            .WithMessage($"Color must not exceed {WorkItemsConstants.ColorMaxLength} characters.")
            .Matches(ColorRegex).WithMessage("Color must be a valid hex color code (e.g. #46A758).")
            .When(x => x.Color is not null);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description must not exceed 1000 characters.")
            .When(x => x.Description is not null);
    }
}
