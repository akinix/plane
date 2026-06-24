using System.Text.RegularExpressions;
using FluentValidation;
using YH.Modules.WorkItems.Contracts;
using YH.Modules.WorkItems.Contracts.v1.States.CreateState;

namespace YH.Modules.WorkItems.Features.v1.States.CreateState;

/// <summary>
/// FluentValidation validator for <see cref="CreateStateCommand"/>.
/// </summary>
public sealed class CreateStateCommandValidator : AbstractValidator<CreateStateCommand>
{
    private static readonly Regex ColorRegex = new("^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

    public CreateStateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("State name is required.")
            .MaximumLength(WorkItemsConstants.NameMaxLength)
            .WithMessage($"State name must not exceed {WorkItemsConstants.NameMaxLength} characters.");

        RuleFor(x => x.Group)
            .InclusiveBetween(0, 4).WithMessage("State group must be between 0 (Backlog) and 4 (Cancelled).");

        RuleFor(x => x.Color)
            .MaximumLength(WorkItemsConstants.ColorMaxLength)
            .WithMessage($"Color must not exceed {WorkItemsConstants.ColorMaxLength} characters.")
            .Matches(ColorRegex).WithMessage("Color must be a valid hex color code (e.g. #F59E0B).")
            .When(x => x.Color is not null);
    }
}
