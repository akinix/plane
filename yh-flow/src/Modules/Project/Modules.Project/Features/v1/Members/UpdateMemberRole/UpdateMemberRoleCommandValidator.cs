using FluentValidation;
using YH.Modules.Project.Contracts.v1.Members.UpdateMemberRole;
using YH.Modules.Workspace.Contracts;

namespace YH.Modules.Project.Features.v1.Members.UpdateMemberRole;

/// <summary>
/// FluentValidation validator for <see cref="UpdateMemberRoleCommand"/>.
/// </summary>
/// <remarks>
/// Enforces the cheap, dependency-free shape rules. The project-level Admin guard is enforced
/// by the endpoint's <c>RequireWorkspaceRole(Admin)</c> and the handler's DbContext lookup.
/// </remarks>
public sealed class UpdateMemberRoleCommandValidator : AbstractValidator<UpdateMemberRoleCommand>
{
    private static readonly int[] AllowedRoles =
        { (int)WorkspaceRole.Guest, (int)WorkspaceRole.Member, (int)WorkspaceRole.Admin };

    public UpdateMemberRoleCommandValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEqual(Guid.Empty).WithMessage("Member id is required.");

        RuleFor(x => x.Role)
            .Must(r => Array.IndexOf(AllowedRoles, r) >= 0)
            .WithMessage("Role must be Guest (5), Member (15), or Admin (20).");
    }
}
