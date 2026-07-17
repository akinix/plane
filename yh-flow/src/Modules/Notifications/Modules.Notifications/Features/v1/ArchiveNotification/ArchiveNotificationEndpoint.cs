using YH.Framework.Shared.Identity.Authorization;
using YH.Modules.Notifications.Contracts.Authorization;
using YH.Modules.Notifications.Contracts.v1.Commands;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Notifications.Features.v1.ArchiveNotification;

public static class ArchiveNotificationEndpoint
{
    internal static RouteHandlerBuilder MapArchiveNotificationEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPut("/{id:guid}/archive",
                async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    await mediator.Send(new ArchiveNotificationCommand(id), cancellationToken);
                    return Results.NoContent();
                })
            .WithName("ArchiveNotification")
            .WithSummary("Archive a single notification")
            .RequirePermission(NotificationPermissions.Inbox.MarkRead);
}
