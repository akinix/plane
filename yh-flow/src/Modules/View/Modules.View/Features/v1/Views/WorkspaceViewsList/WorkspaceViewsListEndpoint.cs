using YH.Modules.View.Contracts.DTOs;
using YH.Modules.View.Contracts.v1.Views.ListViews;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.View.Features.v1.Views.WorkspaceViewsList;

/// <summary>
/// GET /api/v1/workspaces/{slug}/views/ — list workspace-level views (REQ-8.1).
/// Requires workspace Admin or Member role.
/// </summary>
public static class WorkspaceViewsListEndpoint
{
    internal static RouteHandlerBuilder MapWorkspaceViewsListEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/", async ([AsParameters] ListViewsQuery query,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            query.WorkspaceScope = true;
            return TypedResults.Ok(await mediator.Send(query, cancellationToken));
        })
        .WithName("WorkspaceViewsList")
        .WithSummary("List workspace-level views")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("List views at the workspace level (not scoped to a specific project).")
        .Produces<List<ViewDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
