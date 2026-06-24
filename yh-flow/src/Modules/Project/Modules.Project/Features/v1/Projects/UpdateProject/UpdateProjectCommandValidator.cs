using System.Text.RegularExpressions;
using FluentValidation;
using YH.Modules.Project.Contracts;
using YH.Modules.Project.Contracts.v1.Projects.UpdateProject;

namespace YH.Modules.Project.Features.v1.Projects.UpdateProject;

/// <summary>
/// FluentValidation validator for <see cref="UpdateProjectCommand"/>.
/// All fields are optional (PATCH semantics); only provided values are validated.
/// </summary>
public sealed class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    private static readonly Regex ForbiddenCharsPattern =
        new(ProjectConstants.ForbiddenIdentifierCharsPattern, RegexOptions.Compiled);

    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(ProjectConstants.NameMaxLength)
            .WithMessage($"Project name must not exceed {ProjectConstants.NameMaxLength} characters.")
            .Must(name => !ForbiddenCharsPattern.IsMatch(name))
            .WithMessage("Project name contains forbidden characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.Description)
            .MaximumLength(ProjectConstants.DescriptionMaxLength)
            .WithMessage($"Description must not exceed {ProjectConstants.DescriptionMaxLength} characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.TimeZone)
            .MaximumLength(64)
            .WithMessage("Timezone must not exceed 64 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.TimeZone));

        RuleFor(x => x.CoverImageUrl)
            .MaximumLength(2048)
            .WithMessage("Cover image URL must not exceed 2048 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.CoverImageUrl));
    }
}
