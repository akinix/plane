using YH.Framework.Shared.Identity.Authorization;
using YH.Framework.Web.Idempotency;
using YH.Modules.Webhooks.Contracts.Authorization;
using YH.Modules.Webhooks.Contracts.v1.CreateWebhookSubscription;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Webhooks.Features.v1.CreateWebhookSubscription;

public static class CreateWebhookSubscriptionEndpoint
{
    internal static RouteHandlerBuilder MapCreateWebhookSubscriptionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/subscriptions", async (
            CreateWebhookSubscriptionCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            return TypedResults.Created($"/api/v1/webhooks/subscriptions/{id}", id);
        })
        .WithName("CreateWebhookSubscription")
        .WithSummary("Create a webhook subscription")
        .RequirePermission(WebhooksPermissions.Subscriptions.Create)
        .WithIdempotency()
        .Produces<Guid>(StatusCodes.Status201Created);
    }
}
