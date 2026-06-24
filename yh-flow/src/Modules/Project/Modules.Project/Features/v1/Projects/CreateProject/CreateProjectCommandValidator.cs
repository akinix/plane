using System.Text.RegularExpressions;
using FluentValidation;
using YH.Modules.Project.Contracts;
using YH.Modules.Project.Contracts.v1.Projects.CreateProject;

namespace YH.Modules.Project.Features.v1.Projects.CreateProject;

/// <summary>
/// FluentValidation validator for <see cref="CreateProjectCommand"/>.
/// </summary>
/// <remarks>
/// Identifier validation enforces Plane's FORBIDDEN_IDENTIFIER_CHARS_PATTERN
/// (see <see cref="ProjectConstants.ForbiddenIdentifierCharsPattern"/>).
/// Uniqueness checks live in the handler (they need the DbContext which is not available
/// to the validator).
/// </remarks>
public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    private static readonly Regex ForbiddenCharsPattern =
        new(ProjectConstants.ForbiddenIdentifierCharsPattern, RegexOptions.Compiled);

    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(ProjectConstants.NameMaxLength)
            .WithMessage($"Project name must not exceed {ProjectConstants.NameMaxLength} characters.")
            .Must(name => !ForbiddenCharsPattern.IsMatch(name))
            .WithMessage("Project name contains forbidden characters.");

        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage("Project identifier is required.")
            .MaximumLength(ProjectConstants.IdentifierMaxLength)
            .WithMessage($"Project identifier must not exceed {ProjectConstants.IdentifierMaxLength} characters.")
            .Must(id => !ForbiddenCharsPattern.IsMatch(id))
            .WithMessage("Project identifier contains forbidden characters.")
            .Must(id => id.All(char.IsAsciiLetterOrDigit))
            .WithMessage("Project identifier must be alphanumeric.")
            .Must(id => string.Equals(id, id.ToUpperInvariant(), StringComparison.Ordinal))
            .WithMessage("Project identifier must be uppercase.");

        RuleFor(x => x.Description)
            .MaximumLength(ProjectConstants.DescriptionMaxLength)
            .WithMessage($"Description must not exceed {ProjectConstants.DescriptionMaxLength} characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.TimeZone)
            .MaximumLength(64)
            .WithMessage("Timezone must not exceed 64 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.TimeZone));
    }
}
