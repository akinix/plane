using FluentValidation;
using YH.Modules.Identity.Contracts.v1.Sessions.AdminRevokeAllSessions;

namespace YH.Modules.Identity.Features.v1.Sessions.AdminRevokeAllSessions;

public sealed class AdminRevokeAllSessionsCommandValidator : AbstractValidator<AdminRevokeAllSessionsCommand>
{
    public AdminRevokeAllSessionsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.")
            .When(x => x.Reason is not null);
    }
}