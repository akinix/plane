using YH.Modules.View.Contracts.DTOs;
using YH.Modules.View.Contracts.v1.Views.UpdateView;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.View.Features.v1.Views.UpdateView;

/// <summary>
/// PATCH /api/v1/workspaces/{slug}/projects/{projectId}/views/{viewId} — update a view (REQ-8.1).
/// </summary>
public static class UpdateViewEndpoint
{
    private static readonly string[] HttpPatch = ["PATCH"];

    internal static RouteHandlerBuilder MapUpdateViewEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapMethods("/{viewId}", HttpPatch,
            async (Guid viewId, UpdateViewCommand command, IMediator mediator, CancellationToken cancellationToken) =>
            {
                command.ViewId = viewId;
                return TypedResults.Ok(await mediator.Send(command, cancellationToken));
            })
        .WithName("UpdateView")
        .WithSummary("Update view")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Update mutable view fields (name, description, filters, display_filters, display_properties, rich_filters, access, sort_order, logo_props). Workspace Admin or Member required.")
        .Produces<ViewDetailDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
