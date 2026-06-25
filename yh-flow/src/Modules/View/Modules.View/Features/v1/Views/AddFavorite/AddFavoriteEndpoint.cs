using YH.Framework.Core.Exceptions;
using YH.Modules.View.Contracts.v1.Views.AddFavorite;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Framework.Shared.Identity.Claims;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.View.Features.v1.Views.AddFavorite;

/// <summary>
/// POST /api/v1/workspaces/{slug}/projects/{projectId}/views/{viewId}/favorite/ — add a view to user favorites (REQ-8.2).
/// </summary>
public static class AddFavoriteEndpoint
{
    internal static RouteHandlerBuilder MapAddFavoriteViewEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/{viewId}/favorite", async (Guid viewId,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            return TypedResults.Ok(await mediator.Send(new AddFavoriteCommand
            {
                ViewId = viewId,
                UserId = userId,
            }, cancellationToken));
        })
        .WithName("AddFavoriteView")
        .WithSummary("Add view to favorites")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Add a view to the current user's favorites. Idempotent — re-adding an already-favorited view returns true.")
        .Produces<bool>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
