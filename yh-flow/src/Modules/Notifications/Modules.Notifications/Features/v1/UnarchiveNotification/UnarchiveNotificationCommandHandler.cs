using YH.Framework.Core.Context;
using YH.Framework.Core.Exceptions;
using YH.Modules.Notifications.Contracts.v1.Commands;
using YH.Modules.Notifications.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace YH.Modules.Notifications.Features.v1.UnarchiveNotification;

public sealed class UnarchiveNotificationCommandHandler(
    NotificationsDbContext db,
    ICurrentUser currentUser)
    : ICommandHandler<UnarchiveNotificationCommand, Unit>
{
    public async ValueTask<Unit> Handle(UnarchiveNotificationCommand cmd, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty) throw new UnauthorizedException("no current user");
        var currentUserId = userId.ToString();

        var notification = await db.Notifications
            .FirstOrDefaultAsync(n => n.Id == cmd.Id && n.UserId == currentUserId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Notification not found.");

        notification.Unarchive();
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
