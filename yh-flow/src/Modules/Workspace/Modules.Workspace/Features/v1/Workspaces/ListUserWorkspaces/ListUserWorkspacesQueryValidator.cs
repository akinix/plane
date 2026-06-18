using FluentValidation;
using YH.Modules.Workspace.Contracts.v1.Workspaces.ListUserWorkspaces;

namespace YH.Modules.Workspace.Features.v1.Workspaces.ListUserWorkspaces;

/// <summary>
/// Validator for <see cref="ListUserWorkspacesQuery"/> — enforces pagination bounds (Architecture
/// invariant: paginated queries must have a validator).
/// </summary>
public sealed class ListUserWorkspacesQueryValidator : AbstractValidator<ListUserWorkspacesQuery>
{
    public ListUserWorkspacesQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User id is required.");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.")
            .When(x => x.PageNumber.HasValue);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Page size must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.")
            .When(x => x.PageSize.HasValue);

        RuleFor(x => x.BaseUrl)
            .NotEmpty().WithMessage("Base URL is required for pagination links.");
    }
}
