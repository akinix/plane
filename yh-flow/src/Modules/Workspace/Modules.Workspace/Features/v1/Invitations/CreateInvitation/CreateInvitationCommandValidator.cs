using FluentValidation;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Contracts.v1.Invitations.CreateInvitation;

namespace YH.Modules.Workspace.Features.v1.Invitations.CreateInvitation;

/// <summary>
/// FluentValidation validator for <see cref="CreateInvitationCommand"/>.
/// </summary>
public sealed class CreateInvitationCommandValidator : AbstractValidator<CreateInvitationCommand>
{
    private static readonly WorkspaceRole[] AllowedRoles =
        { WorkspaceRole.Guest, WorkspaceRole.Member, WorkspaceRole.Admin };

    public CreateInvitationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.Role)
            .Must(r => Array.IndexOf(AllowedRoles, r) >= 0)
            .WithMessage("Role must be Guest (5), Member (15), or Admin (20).");

        RuleFor(x => x.Message)
            .MaximumLength(2048).WithMessage("Message must not exceed 2048 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Message));
    }
}
