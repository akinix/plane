using YH.Framework.Shared.Identity.Authorization;
using YH.Modules.Notifications.Contracts.Authorization;
using YH.Modules.Notifications.Contracts.v1.Queries;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Notifications.Features.v1.GetNotificationPreferences;

public static class GetNotificationPreferencesEndpoint
{
    internal static RouteHandlerBuilder MapGetNotificationPreferencesEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/preferences",
                async (IMediator mediator, CancellationToken cancellationToken) =>
                    Results.Ok(await mediator.Send(new GetNotificationPreferencesQuery(), cancellationToken)))
            .WithName("GetNotificationPreferences")
            .WithSummary("Get the caller's notification preferences (returns defaults if none set)")
            .RequirePermission(NotificationPermissions.Inbox.View);
}
