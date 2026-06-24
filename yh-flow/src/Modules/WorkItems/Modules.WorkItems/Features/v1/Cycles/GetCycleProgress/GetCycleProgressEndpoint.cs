using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Cycles.GetCycleProgress;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Cycles.GetCycleProgress;

/// <summary>
/// GET /cycles/{cycleId}/progress — get cycle progress/completion chart.
/// Any authenticated user may view cycle progress.
/// </summary>
public static class GetCycleProgressEndpoint
{
    internal static RouteHandlerBuilder MapGetCycleProgressEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{cycleId}/progress", async (Guid projectId, Guid cycleId, string? type,
            IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new GetCycleProgressQuery
            {
                CycleId = cycleId,
                Type = type,
            }, cancellationToken)))
        .WithName("GetCycleProgress")
        .WithSummary("Get cycle progress")
        .RequireAuthorization()
        .WithDescription("Get the completion chart for a cycle. Optional type parameter (default 'issues').")
        .Produces<CycleProgressDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
