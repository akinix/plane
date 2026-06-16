using YH.Framework.Core.Context;
using YH.Framework.Core.Exceptions;
using YH.Modules.Notifications.Contracts.v1.Queries;
using YH.Modules.Notifications.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace YH.Modules.Notifications.Features.v1.GetUnreadCount;

public sealed class GetUnreadCountQueryHandler(
    NotificationsDbContext db,
    ICurrentUser currentUser)
    : IQueryHandler<GetUnreadCountQuery, int>
{
    public async ValueTask<int> Handle(GetUnreadCountQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty) throw new UnauthorizedException("no current user");
        var currentUserId = userId.ToString();

        return await db.Notifications.AsNoTracking()
            .CountAsync(n => n.UserId == currentUserId && n.ReadAtUtc == null, cancellationToken)
            .ConfigureAwait(false);
    }
}
