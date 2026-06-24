using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Cycles.ListCycles;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Cycles.ListCycles;

/// <summary>
/// GET /cycles/ — list cycles in a project.
/// Any authenticated user may list cycles.
/// </summary>
public static class ListCyclesEndpoint
{
    internal static RouteHandlerBuilder MapListCyclesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/", async (Guid projectId, string? cycleView,
            IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new ListCyclesQuery
            {
                ProjectId = projectId,
                CycleView = cycleView,
            }, cancellationToken)))
        .WithName("ListCycles")
        .WithSummary("List cycles")
        .RequireAuthorization()
        .WithDescription("List cycles in a project, ordered by SortOrder ascending. Optional cycle_view filter: current, upcoming, completed, draft, incomplete, all.")
        .Produces<List<CycleDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
