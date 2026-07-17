using YH.Framework.Shared.Identity.Authorization;
using YH.Modules.Notifications.Contracts.Authorization;
using YH.Modules.Notifications.Contracts.v1.Commands;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Notifications.Features.v1.MarkNotificationUnread;

public static class MarkNotificationUnreadEndpoint
{
    internal static RouteHandlerBuilder MapMarkNotificationUnreadEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPut("/{id:guid}/mark-unread",
                async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    await mediator.Send(new MarkNotificationUnreadCommand(id), cancellationToken);
                    return Results.NoContent();
                })
            .WithName("MarkNotificationUnread")
            .WithSummary("Mark a single notification as unread")
            .RequirePermission(NotificationPermissions.Inbox.MarkRead);
}
