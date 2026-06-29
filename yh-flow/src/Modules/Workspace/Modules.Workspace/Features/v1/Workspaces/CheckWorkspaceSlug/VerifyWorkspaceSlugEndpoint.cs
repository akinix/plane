using YH.Modules.Workspace.Contracts.v1.Workspaces.CheckWorkspaceSlug;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Workspace.Features.v1.Workspaces.CheckWorkspaceSlug;

/// <summary>
/// POST /api/v1/workspaces/slug-check/ — check slug availability (REQ-2.1).
/// </summary>
/// <remarks>
/// Top-level endpoint (no <c>{slug}</c>); any authenticated user may probe. Mirrors Plane's
/// <c>POST /api/v1/workspaces/slug-check/</c>.
/// </remarks>
public static class VerifyWorkspaceSlugEndpoint
{
    internal static RouteHandlerBuilder MapVerifyWorkspaceSlugEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/slug-check", async (CheckWorkspaceSlugQuery query, IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(query, cancellationToken)))
        .WithName("VerifyWorkspaceSlug")
        .WithSummary("Check workspace slug availability")
        .RequireAuthorization()
        .WithDescription("Check whether a workspace slug is available. Returns { exists: true/false }. Soft-deleted workspaces release their slug.")
        .Produces<CheckWorkspaceSlugResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
