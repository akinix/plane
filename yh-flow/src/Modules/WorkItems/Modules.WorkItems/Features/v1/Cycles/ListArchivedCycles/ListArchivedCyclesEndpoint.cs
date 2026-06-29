using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Cycles.ArchiveCycle;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Cycles.ListArchivedCycles;

/// <summary>
/// GET /archived-cycles/ — list archived cycles in a project.
/// Any authenticated user may list archived cycles.
/// This endpoint is registered on the <c>archived-cycles</c> route group.
/// </summary>
public static class ListArchivedCyclesEndpoint
{
    internal static RouteHandlerBuilder MapListArchivedCyclesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/", async (Guid projectId,
            IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new ListArchivedCyclesQuery
            {
                ProjectId = projectId,
            }, cancellationToken)))
        .WithName("ListArchivedCycles")
        .WithSummary("List archived cycles")
        .RequireAuthorization()
        .WithDescription("List archived cycles in a project, ordered by archived date descending.")
        .Produces<List<CycleDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
