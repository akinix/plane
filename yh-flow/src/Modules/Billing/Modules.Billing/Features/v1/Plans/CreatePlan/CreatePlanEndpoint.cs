using YH.Modules.Billing.Contracts.Authorization;
using YH.Framework.Shared.Identity.Authorization;
using YH.Framework.Web.Idempotency;
using YH.Modules.Billing.Contracts.v1.Plans;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Billing.Features.v1.Plans.CreatePlan;

public static class CreatePlanEndpoint
{
    internal static RouteHandlerBuilder MapCreatePlanEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/plans",
                async (CreatePlanCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateBillingPlan")
            .WithSummary("Create a new billing plan")
            .RequirePermission(BillingPermissions.Manage)
            .WithIdempotency();
    }
}
