using FluentValidation;
using YH.Modules.Workspace.Contracts.v1.Members.ListMembers;

namespace YH.Modules.Workspace.Features.v1.Members.ListMembers;

/// <summary>
/// FluentValidation validator for <see cref="ListMembersQuery"/>.
/// </summary>
/// <remarks>
/// Shape rules only — exists to satisfy the Architecture.Tests invariant that every paginated
/// query handler has a FluentValidation validator (bound PageNumber/PageSize bounds).
/// </remarks>
public sealed class ListMembersQueryValidator : AbstractValidator<ListMembersQuery>
{
    public ListMembersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.")
            .When(x => x.PageNumber.HasValue);

        RuleFor(x => x.PageSize)
            .GreaterThan(0).LessThanOrEqualTo(100).WithMessage("PageSize must be between 1 and 100.")
            .When(x => x.PageSize.HasValue);
    }
}
