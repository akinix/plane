using FluentValidation;
using YH.Modules.Project.Contracts.v1.Members.RemoveMember;

namespace YH.Modules.Project.Features.v1.Members.RemoveMember;

/// <summary>
/// FluentValidation validator for <see cref="RemoveMemberCommand"/>.
/// </summary>
/// <remarks>
/// Shape rule only. The last-Admin guard lives in the handler (needs the DbContext).
/// </remarks>
public sealed class RemoveMemberCommandValidator : AbstractValidator<RemoveMemberCommand>
{
    public RemoveMemberCommandValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEqual(Guid.Empty).WithMessage("Member id is required.");
    }
}
