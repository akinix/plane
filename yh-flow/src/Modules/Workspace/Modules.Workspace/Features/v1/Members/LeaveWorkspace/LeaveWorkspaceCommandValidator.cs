using FluentValidation;
using YH.Modules.Workspace.Contracts.v1.Members.LeaveWorkspace;

namespace YH.Modules.Workspace.Features.v1.Members.LeaveWorkspace;

/// <summary>
/// FluentValidation validator for <see cref="LeaveWorkspaceCommand"/>.
/// </summary>
public sealed class LeaveWorkspaceCommandValidator : AbstractValidator<LeaveWorkspaceCommand>
{
    public LeaveWorkspaceCommandValidator()
    {
        // WorkspaceId + CurrentUserId are populated by the endpoint (not client-writable) — the
        // handler re-checks them. The validator exists to satisfy the Architecture
        // HandlerValidatorPairing invariant (every command handler needs a validator).
        RuleFor(x => x.WorkspaceId)
            .NotEqual(Guid.Empty).WithMessage("Workspace id is required.");
    }
}
