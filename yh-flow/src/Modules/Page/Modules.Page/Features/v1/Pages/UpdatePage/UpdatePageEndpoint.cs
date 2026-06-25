using YH.Modules.Page.Contracts.DTOs;
using YH.Modules.Page.Contracts.v1.Pages.UpdatePage;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Page.Features.v1.Pages.UpdatePage;

/// <summary>
/// PATCH /api/v1/workspaces/{slug}/projects/{projectId}/pages/{pageId} — update a page (REQ-7.1).
/// </summary>
public static class UpdatePageEndpoint
{
    private static readonly string[] HttpPatch = ["PATCH"];

    internal static RouteHandlerBuilder MapUpdatePageEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapMethods("/{pageId}", HttpPatch,
            async (Guid pageId, UpdatePageCommand command, IMediator mediator, CancellationToken cancellationToken) =>
            {
                command.PageId = pageId;
                return TypedResults.Ok(await mediator.Send(command, cancellationToken));
            })
        .WithName("UpdatePage")
        .WithSummary("Update page")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Update mutable page fields (name, description, access, color, parent, sort_order, view_props, logo_props, is_global). Workspace Admin or Member required.")
        .Produces<PageDetailDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}