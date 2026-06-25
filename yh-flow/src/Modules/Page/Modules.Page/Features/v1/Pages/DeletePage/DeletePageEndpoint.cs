using YH.Modules.Page.Contracts.v1.Pages.DeletePage;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Page.Features.v1.Pages.DeletePage;

/// <summary>
/// DELETE /api/v1/workspaces/{slug}/projects/{projectId}/pages/{pageId} — soft-delete a page (REQ-7.1).
/// </summary>
public static class DeletePageEndpoint
{
    internal static RouteHandlerBuilder MapDeletePageEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{pageId}", async (Guid pageId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeletePageCommand { PageId = pageId }, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("DeletePage")
        .WithSummary("Delete page")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Soft-delete a page. Workspace Admin or Member role required.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}