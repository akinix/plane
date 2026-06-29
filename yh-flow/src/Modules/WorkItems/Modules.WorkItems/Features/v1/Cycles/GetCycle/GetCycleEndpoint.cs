using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Cycles.GetCycle;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Cycles.GetCycle;

/// <summary>
/// GET /cycles/{cycleId}/ — fetch a cycle by id.
/// Any authenticated user may read cycle metadata.
/// </summary>
public static class GetCycleEndpoint
{
    internal static RouteHandlerBuilder MapGetCycleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{cycleId}", async (Guid projectId, Guid cycleId,
            IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new GetCycleQuery
            {
                ProjectId = projectId,
                CycleId = cycleId,
            }, cancellationToken)))
        .WithName("GetCycle")
        .WithSummary("Get cycle by id")
        .RequireAuthorization()
        .WithDescription("Fetch a cycle by id. Any authenticated user may read cycle metadata.")
        .Produces<CycleDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}
