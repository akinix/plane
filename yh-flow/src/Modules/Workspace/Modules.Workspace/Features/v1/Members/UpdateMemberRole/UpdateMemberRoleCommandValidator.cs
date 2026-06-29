using FluentValidation;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Contracts.v1.Members.UpdateMemberRole;

namespace YH.Modules.Workspace.Features.v1.Members.UpdateMemberRole;

/// <summary>
/// FluentValidation validator for <see cref="UpdateMemberRoleCommand"/>.
/// </summary>
/// <remarks>
/// Enforces the cheap, dependency-free shape rules. The self-promotion guard lives in the
/// handler (it needs the target member's <c>UserId</c> which requires a DB lookup).
/// </remarks>
public sealed class UpdateMemberRoleCommandValidator : AbstractValidator<UpdateMemberRoleCommand>
{
    private static readonly WorkspaceRole[] AllowedRoles =
        { WorkspaceRole.Guest, WorkspaceRole.Member, WorkspaceRole.Admin };

    public UpdateMemberRoleCommandValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEqual(Guid.Empty).WithMessage("Member id is required.");

        RuleFor(x => x.Role)
            .Must(r => Array.IndexOf(AllowedRoles, r) >= 0)
            .WithMessage("Role must be Guest (5), Member (15), or Admin (20). None is not permitted.");
    }
}
