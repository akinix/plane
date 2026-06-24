using FluentValidation;
using YH.Modules.WorkItems.Contracts;
using YH.Modules.WorkItems.Contracts.v1.Estimates.CreateEstimate;

namespace YH.Modules.WorkItems.Features.v1.Estimates.CreateEstimate;

/// <summary>
/// FluentValidation validator for <see cref="CreateEstimateCommand"/>.
/// </summary>
public sealed class CreateEstimateCommandValidator : AbstractValidator<CreateEstimateCommand>
{
    public CreateEstimateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Estimate name is required.")
            .MaximumLength(WorkItemsConstants.NameMaxLength)
            .WithMessage($"Estimate name must not exceed {WorkItemsConstants.NameMaxLength} characters.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Estimate type is required.")
            .Must(t => t == "points" || t == "categories")
            .WithMessage("Estimate type must be 'points' or 'categories'.");

        // Validate initial points if provided
        When(x => x.InitialPoints is { Count: > 0 }, () =>
        {
            RuleForEach(x => x.InitialPoints).ChildRules(point =>
            {
                point.RuleFor(p => p.Key)
                    .GreaterThanOrEqualTo(0).WithMessage("Estimate point key must be >= 0.");

                point.RuleFor(p => p.Value)
                    .NotEmpty().WithMessage("Estimate point value is required.")
                    .MaximumLength(50).WithMessage("Estimate point value must not exceed 50 characters.");
            });
        });
    }
}
