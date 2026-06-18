using Mediator;
using YH.Modules.Workspace.Contracts.v1.Members.LeaveWorkspace;
using YH.Modules.Workspace.Services;

namespace YH.Modules.Workspace.Features.v1.Members.LeaveWorkspace;

/// <summary>
/// Handles <see cref="LeaveWorkspaceCommand"/> — self-removal from the resolved workspace
/// (REQ-2.2). Idempotent — calling Leave when already not a member is a no-op.
/// </summary>
public sealed class LeaveWorkspaceCommandHandler : ICommandHandler<LeaveWorkspaceCommand>
{
    private readonly WorkspaceMembershipService _membership;

    public LeaveWorkspaceCommandHandler(WorkspaceMembershipService membership)
    {
        _membership = membership ?? throw new ArgumentNullException(nameof(membership));
    }

    public async ValueTask<Unit> Handle(LeaveWorkspaceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.WorkspaceId == Guid.Empty)
        {
            throw new ArgumentException("Workspace id is required.", nameof(command));
        }

        if (command.CurrentUserId == Guid.Empty)
        {
            throw new ArgumentException("Current user id is required.", nameof(command));
        }

        await _membership.LeaveAsync(command.WorkspaceId, command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return Unit.Value;
    }
}
