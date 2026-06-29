using YH.Modules.Page.Contracts.v1.Pages.ArchivePage;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Page.Features.v1.Pages.ArchivePage;

/// <summary>
/// POST /api/v1/workspaces/{slug}/projects/{projectId}/pages/{pageId}/archive/ — archive a page (REQ-7.1).
/// </summary>
public static class ArchivePageEndpoint
{
    internal static RouteHandlerBuilder MapArchivePageEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/{pageId}/archive", async (Guid pageId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return TypedResults.Ok(await mediator.Send(new ArchivePageCommand { PageId = pageId }, cancellationToken));
        })
        .WithName("ArchivePage")
        .WithSummary("Archive page")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Archive a page. An archived page is hidden from default views but its data is preserved.")
        .Produces<bool>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}