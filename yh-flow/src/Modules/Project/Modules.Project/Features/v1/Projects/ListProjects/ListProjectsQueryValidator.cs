using FluentValidation;
using YH.Modules.Project.Contracts.v1.Projects.ListProjects;

namespace YH.Modules.Project.Features.v1.Projects.ListProjects;

/// <summary>
/// FluentValidation validator for <see cref="ListProjectsQuery"/>.
/// </summary>
public sealed class ListProjectsQueryValidator : AbstractValidator<ListProjectsQuery>
{
    public ListProjectsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be >= 1.")
            .When(x => x.PageNumber.HasValue);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.")
            .When(x => x.PageSize.HasValue);

        RuleFor(x => x.Network)
            .Must(n => n is 0 or 2)
            .WithMessage("Network must be 0 (Secret) or 2 (Public).")
            .When(x => x.Network.HasValue);
    }
}
