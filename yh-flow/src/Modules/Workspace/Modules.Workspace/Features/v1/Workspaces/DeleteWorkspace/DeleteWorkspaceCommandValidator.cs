using FluentValidation;
using YH.Modules.Workspace;
using YH.Modules.Workspace.Contracts.v1.Workspaces.DeleteWorkspace;

namespace YH.Modules.Workspace.Features.v1.Workspaces.DeleteWorkspace;

/// <summary>
/// Validator for <see cref="DeleteWorkspaceCommand"/>. Validates the route-bound slug shape; the
/// owner check is enforced in the handler (it needs DbContext + ICurrentUser).
/// </summary>
public sealed class DeleteWorkspaceCommandValidator : AbstractValidator<DeleteWorkspaceCommand>
{
    public DeleteWorkspaceCommandValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(WorkspaceModuleConstants.SlugMaxLength)
            .WithMessage($"Slug must not exceed {WorkspaceModuleConstants.SlugMaxLength} characters.");

        RuleFor(x => x.CurrentUserId)
            .NotEqual(Guid.Empty).WithMessage("Authenticated user id is required.");
    }
}
