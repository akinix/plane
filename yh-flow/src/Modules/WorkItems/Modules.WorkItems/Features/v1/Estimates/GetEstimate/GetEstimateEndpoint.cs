using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Estimates.GetEstimate;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Estimates.GetEstimate;

/// <summary>
/// GET /estimates/{estimateId}/ — fetch an estimate by id with its points (REQ-4.7).
/// Any authenticated user may read estimate metadata.
/// </summary>
public static class GetEstimateEndpoint
{
    internal static RouteHandlerBuilder MapGetEstimateEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{estimateId}", async (Guid projectId, Guid estimateId,
            IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new GetEstimateQuery
            {
                ProjectId = projectId,
                EstimateId = estimateId,
            }, cancellationToken)))
        .WithName("GetEstimate")
        .WithSummary("Get estimate by id")
        .RequireAuthorization()
        .WithDescription("Fetch an estimate by id with its estimate points sorted by key.")
        .Produces<EstimateDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}
