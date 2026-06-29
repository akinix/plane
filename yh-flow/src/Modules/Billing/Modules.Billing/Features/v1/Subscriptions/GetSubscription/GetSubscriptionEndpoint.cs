using YH.Modules.Billing.Contracts.Authorization;
using YH.Framework.Shared.Identity.Authorization;
using YH.Modules.Billing.Contracts.v1.Subscriptions;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Billing.Features.v1.Subscriptions.GetSubscription;

public static class GetSubscriptionEndpoint
{
    internal static RouteHandlerBuilder MapGetSubscriptionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/subscriptions",
                (string? tenantId, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetSubscriptionQuery(tenantId), ct))
            .WithName("GetSubscription")
            .WithSummary("Get the active subscription for a tenant (admin) or the current tenant")
            .RequirePermission(BillingPermissions.View);
    }

    internal static RouteHandlerBuilder MapGetMySubscriptionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/subscriptions/me",
                (IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetSubscriptionQuery(null), ct))
            .WithName("GetMySubscription")
            .WithSummary("Get the active subscription for the current tenant");
    }
}
