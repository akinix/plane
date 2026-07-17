using YH.Framework.Core.Context;
using YH.Framework.Core.Exceptions;
using YH.Modules.Notifications.Contracts.v1.Commands;
using YH.Modules.Notifications.Data;
using YH.Modules.Notifications.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace YH.Modules.Notifications.Features.v1.UpdateNotificationPreferences;

public sealed class UpdateNotificationPreferencesCommandHandler(
    NotificationsDbContext db,
    ICurrentUser currentUser)
    : ICommandHandler<UpdateNotificationPreferencesCommand, Unit>
{
    public async ValueTask<Unit> Handle(UpdateNotificationPreferencesCommand cmd, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty) throw new UnauthorizedException("no current user");

        var preference = await db.NotificationPreferences
            .FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.WorkspaceId == cmd.WorkspaceId &&
                p.ProjectId == cmd.ProjectId,
                cancellationToken)
            .ConfigureAwait(false);

        if (preference is null)
        {
            preference = UserNotificationPreference.Create(userId, cmd.WorkspaceId, cmd.ProjectId);
            db.NotificationPreferences.Add(preference);
        }

        preference.Update(
            cmd.PropertyChanged,
            cmd.StateChanged,
            cmd.Comment,
            cmd.Mention,
            cmd.IssueCompleted);

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
