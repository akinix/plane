using FluentValidation;
using YH.Modules.Workspace;
using YH.Modules.Workspace.Contracts.v1.Workspaces.CreateWorkspace;

namespace YH.Modules.Workspace.Features.v1.Workspaces.CreateWorkspace;

/// <summary>
/// FluentValidation validator for <see cref="CreateWorkspaceCommand"/>.
/// </summary>
/// <remarks>
/// Restricted-words and final uniqueness checks live in the handler (they need
/// <c>ISlugGenerator</c> which is not available to the validator). The validator enforces the
/// cheap, dependency-free shape rules.
/// </remarks>
public sealed class CreateWorkspaceCommandValidator : AbstractValidator<CreateWorkspaceCommand>
{
    public CreateWorkspaceCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Workspace name is required.")
            .MaximumLength(80).WithMessage("Workspace name must not exceed 80 characters.");

        // Slug: optional. When provided, enforce format shape (length + char class) — restricted
        // words + uniqueness are validated by the handler via ISlugGenerator.
        RuleFor(x => x.Slug)
            .MaximumLength(WorkspaceModuleConstants.SlugMaxLength)
            .WithMessage($"Slug must not exceed {WorkspaceModuleConstants.SlugMaxLength} characters.")
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase alphanumeric with single dashes between groups.")
            .When(x => !string.IsNullOrWhiteSpace(x.Slug));

        RuleFor(x => x.TimeZone)
            .MaximumLength(64)
            .WithMessage("Timezone must not exceed 64 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.TimeZone));

        RuleFor(x => x.Logo)
            .MaximumLength(2048)
            .WithMessage("Logo URL must not exceed 2048 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Logo));
    }
}
