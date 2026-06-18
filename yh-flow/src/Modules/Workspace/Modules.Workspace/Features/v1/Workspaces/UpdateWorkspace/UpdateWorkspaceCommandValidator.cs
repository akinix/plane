using FluentValidation;
using YH.Modules.Workspace.Contracts.v1.Workspaces.UpdateWorkspace;

namespace YH.Modules.Workspace.Features.v1.Workspaces.UpdateWorkspace;

/// <summary>
/// Validator for <see cref="UpdateWorkspaceCommand"/>. All fields are optional (PATCH semantics);
/// when present, they must satisfy shape rules.
/// </summary>
public sealed class UpdateWorkspaceCommandValidator : AbstractValidator<UpdateWorkspaceCommand>
{
    public UpdateWorkspaceCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Workspace name cannot be empty when provided.")
            .MaximumLength(80).WithMessage("Workspace name must not exceed 80 characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.TimeZone)
            .MaximumLength(64)
            .When(x => !string.IsNullOrWhiteSpace(x.TimeZone));

        RuleFor(x => x.Logo)
            .MaximumLength(2048)
            .When(x => !string.IsNullOrWhiteSpace(x.Logo));

        RuleFor(x => x.OrganizationSize)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.OrganizationSize));

        RuleFor(x => x.BackgroundColor)
            .MaximumLength(32)
            .When(x => !string.IsNullOrWhiteSpace(x.BackgroundColor));

        RuleFor(x => x.Description)
            .MaximumLength(2048)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
