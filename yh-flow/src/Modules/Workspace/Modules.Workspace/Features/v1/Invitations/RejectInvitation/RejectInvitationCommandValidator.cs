using FluentValidation;
using YH.Modules.Workspace.Contracts.v1.Invitations.RejectInvitation;

namespace YH.Modules.Workspace.Features.v1.Invitations.RejectInvitation;

/// <summary>
/// FluentValidation validator for <see cref="RejectInvitationCommand"/>.
/// </summary>
public sealed class RejectInvitationCommandValidator : AbstractValidator<RejectInvitationCommand>
{
    public RejectInvitationCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.")
            .MaximumLength(256).WithMessage("Token must not exceed 256 characters.");
    }
}
