using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.States.GetState;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.States.GetState;

/// <summary>
/// GET /states/{stateId}/ — fetch a state by id (REQ-4.1).
/// Any authenticated user may read state metadata.
/// </summary>
public static class GetStateEndpoint
{
    internal static RouteHandlerBuilder MapGetStateEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{stateId}", async (Guid projectId, Guid stateId,
            IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new GetStateQuery
            {
                ProjectId = projectId,
                StateId = stateId,
            }, cancellationToken)))
        .WithName("GetState")
        .WithSummary("Get state by id")
        .RequireAuthorization()
        .WithDescription("Fetch a state by id. Any authenticated user may read state metadata.")
        .Produces<StateDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}
