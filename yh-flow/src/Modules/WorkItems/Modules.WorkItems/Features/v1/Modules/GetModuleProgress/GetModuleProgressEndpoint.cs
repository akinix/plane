using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Modules.GetModuleProgress;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Modules.GetModuleProgress;

/// <summary>
/// GET /modules/{moduleId}/progress — get real-time module progress (aggregated issue counts).
/// Any authenticated user may view module progress.
/// </summary>
public static class GetModuleProgressEndpoint
{
    internal static RouteHandlerBuilder MapGetModuleProgressEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{moduleId}/progress", async (Guid projectId, Guid moduleId,
            IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new GetModuleProgressQuery
            {
                ProjectId = projectId,
                ModuleId = moduleId,
            }, cancellationToken)))
        .WithName("GetModuleProgress")
        .WithSummary("Get module progress")
        .RequireAuthorization()
        .WithDescription("Get real-time module progress with aggregated issue counts grouped by state (total/completed/cancelled/started/unstarted/backlog) and completion percentage.")
        .Produces<ModuleProgressDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
