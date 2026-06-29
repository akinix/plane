using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Labels.ListLabels;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Labels.ListLabels;

/// <summary>
/// GET /labels/ — list labels in a project (REQ-4.2).
/// Returns flat list ordered by SortOrder. Supports optional ParentId filter.
/// Any authenticated user may list labels.
/// </summary>
public static class ListLabelsEndpoint
{
    internal static RouteHandlerBuilder MapListLabelsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/", async (Guid projectId, Guid? parentId,
            IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new ListLabelsQuery
            {
                ProjectId = projectId,
                ParentId = parentId,
            }, cancellationToken)))
        .WithName("ListLabels")
        .WithSummary("List labels")
        .RequireAuthorization()
        .WithDescription("List labels in a project, ordered by SortOrder ascending. Optional ParentId filter for hierarchical queries. If no ParentId filter, returns all labels flat (Plane behavior).")
        .Produces<List<LabelDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
