using YH.Modules.Project.Contracts.DTOs;
using YH.Modules.Project.Contracts.v1.Projects.ListProjects;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Framework.Shared.Identity.Claims;
using YH.Framework.Shared.Persistence;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.Project.Features.v1.Projects.ListProjects;

/// <summary>
/// GET /api/v1/workspaces/{slug}/projects/ — list projects in a workspace (REQ-3.1).
/// Workspace members see all projects; non-project-members see only Public projects (network filter).
/// </summary>
public static class ListProjectsEndpoint
{
    internal static RouteHandlerBuilder MapListProjectsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/", async (string slug,
            HttpRequest request,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return TypedResults.Ok(new PlanePagedResult<ProjectDto>());
            }

            // Plane-compatible pagination params.
            int? pageNumber = TryParseInt(request.Query["page"]);
            int? pageSize = TryParseInt(request.Query["per_page"]) ?? TryParseInt(request.Query["page_size"]);
            int? network = TryParseInt(request.Query["network"]);
            string? orderBy = request.Query["order_by"];

            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}/api/v1/workspaces/{slug}/projects/";

            var result = await mediator.Send(new ListProjectsQuery
            {
                WorkspaceSlug = slug,
                CurrentUserId = userId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                BaseUrl = baseUrl,
                Network = network,
                OrderBy = orderBy,
            }, cancellationToken);
            return TypedResults.Ok(result);
        })
        .WithName("ListProjects")
        .WithSummary("List projects")
        .RequireWorkspaceRole(WorkspaceRole.Guest, WorkspaceRole.Member, WorkspaceRole.Admin)
        .WithDescription("List projects in a workspace. Workspace members see all projects; non-members see only Public projects (network=2). Plane-compatible paginated response.")
        .Produces<PlanePagedResult<ProjectDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }

    private static int? TryParseInt(string? value) =>
        int.TryParse(value, out var parsed) ? parsed : null;
}
