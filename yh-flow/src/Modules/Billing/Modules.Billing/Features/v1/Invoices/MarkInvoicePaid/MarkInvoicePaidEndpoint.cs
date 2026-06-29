using YH.Modules.Billing.Contracts.Authorization;
using YH.Framework.Shared.Identity.Authorization;
using YH.Framework.Web.Idempotency;
using YH.Modules.Billing.Contracts.v1.Invoices;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Billing.Features.v1.Invoices.MarkInvoicePaid;

public static class MarkInvoicePaidEndpoint
{
    internal static RouteHandlerBuilder MapMarkInvoicePaidEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/invoices/{invoiceId:guid}/pay",
                async (Guid invoiceId, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new MarkInvoicePaidCommand(invoiceId), ct)))
            .WithName("MarkInvoicePaid")
            .WithSummary("Mark an issued invoice as paid (manual, no payment processor)")
            .RequirePermission(BillingPermissions.Manage)
            .WithIdempotency();
    }
}
