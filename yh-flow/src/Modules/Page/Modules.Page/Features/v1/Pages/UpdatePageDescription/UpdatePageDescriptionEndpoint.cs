using YH.Modules.Page.Contracts.DTOs;
using YH.Modules.Page.Contracts.v1.Pages.UpdatePageDescription;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Page.Features.v1.Pages.UpdatePageDescription;

/// <summary>
/// PATCH /api/v1/workspaces/{slug}/projects/{projectId}/pages/{pageId}/description/ — update page description (REQ-7.2).
/// </summary>
public static class UpdatePageDescriptionEndpoint
{
    private static readonly string[] HttpPatch = ["PATCH"];

    internal static RouteHandlerBuilder MapUpdatePageDescriptionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapMethods("/{pageId}/description", HttpPatch,
            async (Guid pageId, UpdatePageDescriptionCommand command, IMediator mediator, CancellationToken cancellationToken) =>
            {
                command.PageId = pageId;
                return TypedResults.Ok(await mediator.Send(command, cancellationToken));
            })
        .WithName("UpdatePageDescription")
        .WithSummary("Update page description")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Update a page's description content (description_html, description_stripped, description_json). Workspace Admin or Member required.")
        .Produces<PageDetailDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}