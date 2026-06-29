using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Estimates.ListEstimates;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Estimates.ListEstimates;

/// <summary>
/// GET /estimates/ — list estimate systems for a project (REQ-4.7).
/// Returns all estimates with their points. Any authenticated user may list.
/// </summary>
public static class ListEstimatesEndpoint
{
    internal static RouteHandlerBuilder MapListEstimatesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/", async (Guid projectId,
            IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new ListEstimatesQuery
            {
                ProjectId = projectId,
            }, cancellationToken)))
        .WithName("ListEstimates")
        .WithSummary("List estimates")
        .RequireAuthorization()
        .WithDescription("List all estimate systems for a project, each with its points sorted by key.")
        .Produces<List<EstimateDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
