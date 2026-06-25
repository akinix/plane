using YH.Modules.Page.Contracts.DTOs;
using YH.Modules.Page.Contracts.v1.Pages.ListPages;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Page.Features.v1.Pages.ListPages;

/// <summary>
/// GET /api/v1/workspaces/{slug}/projects/{projectId}/pages/ — list pages in a project (REQ-7.1).
/// Defaults to top-level (parent == null), active (not archived) pages. Requires workspace Admin or Member role.
/// </summary>
public static class ListPagesEndpoint
{
    internal static RouteHandlerBuilder MapListPagesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/", async (Guid projectId,
            [AsParameters] ListPagesQuery query,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            query.ProjectId = projectId;
            return TypedResults.Ok(await mediator.Send(query, cancellationToken));
        })
        .WithName("ListPages")
        .WithSummary("List pages")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("List pages in a project. Defaults to top-level (parent == null), active (not archived) pages. Use ?is_archived=true to show archived pages. Use ?parent=null to include all pages regardless of parent.")
        .Produces<List<PageDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}