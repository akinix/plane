using FluentValidation;
using YH.Modules.Project.Contracts.v1.Members.AddMember;
using YH.Modules.Workspace.Contracts;

namespace YH.Modules.Project.Features.v1.Members.AddMember;

/// <summary>
/// FluentValidation validator for <see cref="AddMemberCommand"/>.
/// </summary>
/// <remarks>
/// Enforces shape rules. Duplicate detection lives in the handler (needs the DbContext).
/// </remarks>
public sealed class AddMemberCommandValidator : AbstractValidator<AddMemberCommand>
{
    private static readonly int[] AllowedRoles =
        { (int)WorkspaceRole.Guest, (int)WorkspaceRole.Member, (int)WorkspaceRole.Admin };

    public AddMemberCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project id is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User id is required.");

        RuleFor(x => x.Role)
            .Must(r => Array.IndexOf(AllowedRoles, r) >= 0)
            .WithMessage("Role must be Guest (5), Member (15), or Admin (20).");
    }
}
