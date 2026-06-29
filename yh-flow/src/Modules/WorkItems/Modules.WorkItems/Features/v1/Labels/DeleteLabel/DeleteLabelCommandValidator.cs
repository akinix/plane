using FluentValidation;
using YH.Modules.WorkItems.Contracts.v1.Labels.DeleteLabel;

namespace YH.Modules.WorkItems.Features.v1.Labels.DeleteLabel;

/// <summary>
/// FluentValidation validator for <see cref="DeleteLabelCommand"/>.
/// </summary>
public sealed class DeleteLabelCommandValidator : AbstractValidator<DeleteLabelCommand>
{
    public DeleteLabelCommandValidator()
    {
        RuleFor(x => x.LabelId)
            .NotEmpty().WithMessage("Label id is required.");
    }
}
