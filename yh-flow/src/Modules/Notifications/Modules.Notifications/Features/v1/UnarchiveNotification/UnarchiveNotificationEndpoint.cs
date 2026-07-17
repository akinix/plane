using YH.Framework.Shared.Identity.Authorization;
using YH.Modules.Notifications.Contracts.Authorization;
using YH.Modules.Notifications.Contracts.v1.Commands;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Notifications.Features.v1.UnarchiveNotification;

public static class UnarchiveNotificationEndpoint
{
    internal static RouteHandlerBuilder MapUnarchiveNotificationEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPut("/{id:guid}/unarchive",
                async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    await mediator.Send(new UnarchiveNotificationCommand(id), cancellationToken);
                    return Results.NoContent();
                })
            .WithName("UnarchiveNotification")
            .WithSummary("Unarchive a single notification")
            .RequirePermission(NotificationPermissions.Inbox.MarkRead);
}
