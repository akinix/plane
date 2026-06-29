using YH.Framework.Core.Exceptions;
using YH.Modules.Page.Contracts.v1.Pages.RemoveFavorite;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Framework.Shared.Identity.Claims;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.Page.Features.v1.Pages.RemoveFavorite;

/// <summary>
/// DELETE /api/v1/workspaces/{slug}/projects/{projectId}/pages/{pageId}/favorite/ — remove a page from user favorites (REQ-7.2).
/// </summary>
public static class RemoveFavoriteEndpoint
{
    internal static RouteHandlerBuilder MapRemoveFavoriteEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{pageId}/favorite", async (Guid pageId,
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
                PageId = pageId,
                UserId = userId,
            }, cancellationToken));
        })
        .WithName("RemoveFavorite")
        .WithSummary("Remove page from favorites")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Remove a page from the current user's favorites. Idempotent — removing a non-favorited page returns true.")
        .Produces<bool>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}