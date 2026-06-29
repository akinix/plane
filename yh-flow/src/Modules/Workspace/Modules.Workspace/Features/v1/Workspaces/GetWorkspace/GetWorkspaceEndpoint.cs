using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Contracts.DTOs;
using YH.Modules.Workspace.Contracts.v1.Workspaces.GetWorkspace;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Workspace.Features.v1.Workspaces.GetWorkspace;

/// <summary>
/// GET /api/v1/workspaces/{slug} — fetch a workspace by slug (REQ-2.1).
/// </summary>
/// <remarks>
/// Plain <c>.RequireAuthorization()</c> (any authenticated user; Plane permits public read of
/// workspace metadata). <c>[RequireWorkspaceRole]</c> is intentionally NOT applied — non-members
/// may read.
/// </remarks>
public static class GetWorkspaceEndpoint
{
    internal static RouteHandlerBuilder MapGetWorkspaceEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{slug}", async (string slug, IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new GetWorkspaceQuery { Slug = slug }, cancellationToken)))
        .WithName("GetWorkspace")
        .WithSummary("Get workspace by slug")
        .RequireAuthorization()
        .WithDescription("Fetch a workspace by slug. Any authenticated user may read workspace metadata.")
        .Produces<WorkspaceDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}
