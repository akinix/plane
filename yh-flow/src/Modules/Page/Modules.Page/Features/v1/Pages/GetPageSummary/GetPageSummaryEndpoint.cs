using YH.Modules.Page.Contracts.v1.Pages.GetPageSummary;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Page.Features.v1.Pages.GetPageSummary;

/// <summary>
/// GET /api/v1/workspaces/{slug}/projects/{projectId}/pages-summary/ — get page summary (REQ-7.1).
/// Returns counts and recently updated pages. Requires workspace Admin or Member role.
/// </summary>
public static class GetPageSummaryEndpoint
{
    internal static RouteHandlerBuilder MapGetPageSummaryEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/", async (Guid projectId,
            [AsParameters] GetPageSummaryQuery query,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            query.ProjectId = projectId;
            return TypedResults.Ok(await mediator.Send(query, cancellationToken));
        })
        .WithName("GetPageSummary")
        .WithSummary("Get page summary")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Get a project-level page summary: total page count, archived count, and recently updated pages.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}