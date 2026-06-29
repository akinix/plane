using System.Text.RegularExpressions;
using FluentValidation;
using YH.Modules.WorkItems.Contracts;
using YH.Modules.WorkItems.Contracts.v1.States.UpdateState;

namespace YH.Modules.WorkItems.Features.v1.States.UpdateState;

/// <summary>
/// FluentValidation validator for <see cref="UpdateStateCommand"/>.
/// All fields are optional (PATCH semantics); only provided values are validated.
/// </summary>
public sealed class UpdateStateCommandValidator : AbstractValidator<UpdateStateCommand>
{
    private static readonly Regex ColorRegex = new("^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

    public UpdateStateCommandValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(WorkItemsConstants.NameMaxLength)
            .WithMessage($"State name must not exceed {WorkItemsConstants.NameMaxLength} characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.Color)
            .MaximumLength(WorkItemsConstants.ColorMaxLength)
            .WithMessage($"Color must not exceed {WorkItemsConstants.ColorMaxLength} characters.")
            .Matches(ColorRegex).WithMessage("Color must be a valid hex color code (e.g. #F59E0B).")
            .When(x => x.Color is not null);
    }
}
