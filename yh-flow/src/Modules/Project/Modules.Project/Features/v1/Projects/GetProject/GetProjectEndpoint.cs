using YH.Modules.Project.Contracts.DTOs;
using YH.Modules.Project.Contracts.v1.Projects.GetProject;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Project.Features.v1.Projects.GetProject;

/// <summary>
/// GET /api/v1/workspaces/{slug}/projects/{projectId} — fetch a project by id (REQ-3.1).
/// Any authenticated user may read project metadata.
/// </summary>
public static class GetProjectEndpoint
{
    internal static RouteHandlerBuilder MapGetProjectEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{projectId}", async (Guid projectId, IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new GetProjectQuery { ProjectId = projectId }, cancellationToken)))
        .WithName("GetProject")
        .WithSummary("Get project by id")
        .RequireAuthorization()
        .WithDescription("Fetch a project by id. Any authenticated user may read project metadata.")
        .Produces<ProjectDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}
