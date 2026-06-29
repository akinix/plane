using YH.Modules.View.Contracts.DTOs;
using YH.Modules.View.Contracts.v1.Views.ListViews;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.View.Features.v1.Views.ListViews;

/// <summary>
/// GET /api/v1/workspaces/{slug}/projects/{projectId}/views/ — list project-level views (REQ-8.1).
/// Requires workspace Admin or Member role.
/// </summary>
public static class ListViewsEndpoint
{
    internal static RouteHandlerBuilder MapListViewsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/", async (Guid projectId,
            [AsParameters] ListViewsQuery query,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            query.ProjectId = projectId;
            return TypedResults.Ok(await mediator.Send(query, cancellationToken));
        })
        .WithName("ListViews")
        .WithSummary("List project-level views")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("List views in a project. Project-level views are those scoped to a specific project.")
        .Produces<List<ViewDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
