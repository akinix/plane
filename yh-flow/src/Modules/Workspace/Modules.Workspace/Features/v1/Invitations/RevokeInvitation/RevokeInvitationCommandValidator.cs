using FluentValidation;
using YH.Modules.Workspace.Contracts.v1.Invitations.RevokeInvitation;

namespace YH.Modules.Workspace.Features.v1.Invitations.RevokeInvitation;

/// <summary>
/// FluentValidation validator for <see cref="RevokeInvitationCommand"/>.
/// </summary>
public sealed class RevokeInvitationCommandValidator : AbstractValidator<RevokeInvitationCommand>
{
    public RevokeInvitationCommandValidator()
    {
        RuleFor(x => x.InvitationId)
            .NotEqual(Guid.Empty).WithMessage("Invitation id is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(512).WithMessage("Reason must not exceed 512 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Reason));
    }
}
