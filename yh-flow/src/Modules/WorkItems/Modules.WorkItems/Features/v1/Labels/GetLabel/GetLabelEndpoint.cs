using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Labels.GetLabel;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Labels.GetLabel;

/// <summary>
/// GET /labels/{labelId}/ — fetch a label by id (REQ-4.2).
/// Any authenticated user may read label metadata.
/// </summary>
public static class GetLabelEndpoint
{
    internal static RouteHandlerBuilder MapGetLabelEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{labelId}", async (Guid projectId, Guid labelId,
            IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new GetLabelQuery
            {
                ProjectId = projectId,
                LabelId = labelId,
            }, cancellationToken)))
        .WithName("GetLabel")
        .WithSummary("Get label by id")
        .RequireAuthorization()
        .WithDescription("Fetch a label by id. Any authenticated user may read label metadata.")
        .Produces<LabelDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}
