using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.States.ListStates;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.States.ListStates;

/// <summary>
/// GET /states/ — list states in a project (REQ-4.1).
/// Any authenticated user may list states.
/// </summary>
public static class ListStatesEndpoint
{
    internal static RouteHandlerBuilder MapListStatesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/", async (Guid projectId, int? group,
            IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new ListStatesQuery
            {
                ProjectId = projectId,
                Group = group,
            }, cancellationToken)))
        .WithName("ListStates")
        .WithSummary("List states")
        .RequireAuthorization()
        .WithDescription("List states in a project, ordered by SortOrder ascending. Optional Group filter (Backlog=0/Unstarted=1/Started=2/Completed=3/Cancelled=4).")
        .Produces<List<StateDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
