using FluentValidation;
using YH.Modules.Workspace.Contracts.v1.Invitations.ListInvitations;

namespace YH.Modules.Workspace.Features.v1.Invitations.ListInvitations;

/// <summary>
/// FluentValidation validator for <see cref="ListInvitationsQuery"/>.
/// </summary>
public sealed class ListInvitationsQueryValidator : AbstractValidator<ListInvitationsQuery>
{
    public ListInvitationsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.")
            .When(x => x.PageNumber.HasValue);

        RuleFor(x => x.PageSize)
            .GreaterThan(0).LessThanOrEqualTo(100).WithMessage("PageSize must be between 1 and 100.")
            .When(x => x.PageSize.HasValue);
    }
}
