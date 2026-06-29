using FluentValidation;
using YH.Modules.Workspace.Contracts.v1.Invitations.AcceptInvitation;

namespace YH.Modules.Workspace.Features.v1.Invitations.AcceptInvitation;

/// <summary>
/// FluentValidation validator for <see cref="AcceptInvitationCommand"/>.
/// </summary>
public sealed class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.")
            .MaximumLength(256).WithMessage("Token must not exceed 256 characters.");
    }
}
