using FluentValidation;
using YH.Modules.Workspace.Contracts.v1.Members.RemoveMember;

namespace YH.Modules.Workspace.Features.v1.Members.RemoveMember;

/// <summary>
/// FluentValidation validator for <see cref="RemoveMemberCommand"/>.
/// </summary>
/// <remarks>
/// Shape rule only. The self-removal guard lives in the handler (needs the target's UserId).
/// </remarks>
public sealed class RemoveMemberCommandValidator : AbstractValidator<RemoveMemberCommand>
{
    public RemoveMemberCommandValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEqual(Guid.Empty).WithMessage("Member id is required.");
    }
}
