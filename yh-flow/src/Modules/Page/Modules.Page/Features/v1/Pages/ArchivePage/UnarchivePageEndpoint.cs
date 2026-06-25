using YH.Modules.Page.Contracts.v1.Pages.ArchivePage;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Page.Features.v1.Pages.ArchivePage;

/// <summary>
/// DELETE /api/v1/workspaces/{slug}/projects/{projectId}/pages/{pageId}/archive/ — unarchive (restore) a page (REQ-7.1).
/// </summary>
public static class UnarchivePageEndpoint
{
    internal static RouteHandlerBuilder MapUnarchivePageEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{pageId}/archive", async (Guid pageId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return TypedResults.Ok(await mediator.Send(new UnarchivePageCommand { PageId = pageId }, cancellationToken));
        })
        .WithName("UnarchivePage")
        .WithSummary("Unarchive page")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Restore an archived page back to active status.")
        .Produces<bool>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}