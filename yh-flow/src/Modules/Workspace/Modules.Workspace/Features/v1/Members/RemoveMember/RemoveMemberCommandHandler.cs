using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Workspace.Contracts.v1.Members.RemoveMember;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.Services;

namespace YH.Modules.Workspace.Features.v1.Members.RemoveMember;

/// <summary>
/// Handles <see cref="RemoveMemberCommand"/> — admin-driven deactivation of a member
/// (REQ-2.2). Rejects self-removal (the caller should use the Leave endpoint instead) so the
/// audit trail distinguishes admin-removal from self-leave.
/// </summary>
public sealed class RemoveMemberCommandHandler : ICommandHandler<RemoveMemberCommand>
{
    private readonly WorkspaceDbContext _db;
    private readonly WorkspaceMembershipService _membership;

    public RemoveMemberCommandHandler(
        WorkspaceDbContext db,
        WorkspaceMembershipService membership)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _membership = membership ?? throw new ArgumentNullException(nameof(membership));
    }

    public async ValueTask<Unit> Handle(RemoveMemberCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.MemberId == Guid.Empty)
        {
            throw new ArgumentException("Member id is required.", nameof(command));
        }

        // Fetch the target to enforce the self-removal guard. NotFound if missing or in a
        // different tenant (the tenant filter hides it; the message is deliberately generic).
        var target = await _db.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == command.MemberId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Member '{command.MemberId}' was not found.");

        // Self-removal is rejected — admins must use the Leave endpoint. This keeps the audit
        // trail unambiguous about which actor drove the deactivation.
        if (Guid.TryParse(target.UserId, out var targetUserId)
            && targetUserId == command.CurrentUserId)
        {
            throw new ForbiddenException("Cannot remove self. Use the leave endpoint instead.");
        }

        await _membership.RemoveAsync(command.MemberId, cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
