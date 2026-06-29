using YH.Framework.Core.Exceptions;
using YH.Modules.View.Contracts.v1.Views.RemoveFavorite;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Framework.Shared.Identity.Claims;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.View.Features.v1.Views.RemoveFavorite;

/// <summary>
/// DELETE /api/v1/workspaces/{slug}/projects/{projectId}/views/{viewId}/favorite/ — remove a view from user favorites (REQ-8.2).
/// </summary>
public static class RemoveFavoriteEndpoint
{
    internal static RouteHandlerBuilder MapRemoveFavoriteViewEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{viewId}/favorite", async (Guid viewId,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            return TypedResults.Ok(await mediator.Send(new RemoveFavoriteCommand
            {
                ViewId = viewId,
                UserId = userId,
            }, cancellationToken));
        })
        .WithName("RemoveFavoriteView")
        .WithSummary("Remove view from favorites")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Remove a view from the current user's favorites. Idempotent — removing a non-favorited view returns true.")
        .Produces<bool>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
