using YH.Framework.Core.Context;
using YH.Framework.Core.Exceptions;
using YH.Modules.Notifications.Contracts.v1.DTOs;
using YH.Modules.Notifications.Contracts.v1.Queries;
using YH.Modules.Notifications.Data;
using YH.Modules.Notifications.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace YH.Modules.Notifications.Features.v1.GetNotificationPreferences;

public sealed class GetNotificationPreferencesQueryHandler(
    NotificationsDbContext db,
    ICurrentUser currentUser)
    : IQueryHandler<GetNotificationPreferencesQuery, UserNotificationPreferenceDto>
{
    public async ValueTask<UserNotificationPreferenceDto> Handle(GetNotificationPreferencesQuery q, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(q);
        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty) throw new UnauthorizedException("no current user");

        var preference = await db.NotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        if (preference is not null)
        {
            return new UserNotificationPreferenceDto
            {
                Id = preference.Id,
                UserId = preference.UserId,
                WorkspaceId = preference.WorkspaceId,
                ProjectId = preference.ProjectId,
                PropertyChanged = preference.PropertyChanged,
                StateChanged = preference.StateChanged,
                Comment = preference.Comment,
                Mention = preference.Mention,
                IssueCompleted = preference.IssueCompleted,
            };
        }

        // Return default (all-on) preferences if none exist yet.
        return new UserNotificationPreferenceDto
        {
            Id = Guid.Empty,
            UserId = userId,
            WorkspaceId = null,
            ProjectId = null,
            PropertyChanged = true,
            StateChanged = true,
            Comment = true,
            Mention = true,
            IssueCompleted = true,
        };
    }
}
