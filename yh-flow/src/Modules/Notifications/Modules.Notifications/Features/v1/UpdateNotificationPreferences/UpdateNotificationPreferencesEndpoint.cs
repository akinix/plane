using YH.Framework.Shared.Identity.Authorization;
using YH.Modules.Notifications.Contracts.Authorization;
using YH.Modules.Notifications.Contracts.v1.Commands;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Notifications.Features.v1.UpdateNotificationPreferences;

public static class UpdateNotificationPreferencesEndpoint
{
    internal static RouteHandlerBuilder MapUpdateNotificationPreferencesEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPut("/preferences",
                async (UpdateNotificationPreferencesCommand cmd, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    await mediator.Send(cmd, cancellationToken);
                    return Results.NoContent();
                })
            .WithName("UpdateNotificationPreferences")
            .WithSummary("Update the caller's notification preferences")
            .RequirePermission(NotificationPermissions.Inbox.View);
}
