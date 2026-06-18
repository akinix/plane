using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Workspace.Contracts.v1.Invitations.RevokeInvitation;
using YH.Modules.Workspace.Data;

namespace YH.Modules.Workspace.Features.v1.Invitations.RevokeInvitation;

/// <summary>
/// Handles <see cref="RevokeInvitationCommand"/> — admin-driven revocation of a pending
/// invitation (REQ-2.4, threat T-2-replay [BLOCKING]). The <c>Revoke()</c> transition stamps
/// <c>RespondedAt</c>; subsequent <c>ValidateAsync</c> calls on the same token return null.
/// </summary>
public sealed class RevokeInvitationCommandHandler : ICommandHandler<RevokeInvitationCommand>
{
    private readonly WorkspaceDbContext _db;

    public RevokeInvitationCommandHandler(WorkspaceDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(RevokeInvitationCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.InvitationId == Guid.Empty)
        {
            throw new ArgumentException("Invitation id is required.", nameof(command));
        }

        var invitation = await _db.Invitations
            .FirstOrDefaultAsync(i => i.Id == command.InvitationId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Invitation '{command.InvitationId}' was not found.");

        // Revoke() is idempotent — returns false if already terminal; we do not surface that
        // as an error (revoking an already-revoked invitation is a no-op success).
        invitation.Revoke(command.Reason);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
