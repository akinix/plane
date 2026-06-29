using YH.Framework.Core.Exceptions;
using YH.Modules.Page.Contracts.v1.Pages.AddFavorite;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Framework.Shared.Identity.Claims;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.Page.Features.v1.Pages.AddFavorite;

/// <summary>
/// POST /api/v1/workspaces/{slug}/projects/{projectId}/pages/{pageId}/favorite/ — add a page to user favorites (REQ-7.2).
/// </summary>
public static class AddFavoriteEndpoint
{
    internal static RouteHandlerBuilder MapAddFavoriteEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/{pageId}/favorite", async (Guid pageId,
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
                PageId = pageId,
                UserId = userId,
            }, cancellationToken));
        })
        .WithName("AddFavorite")
        .WithSummary("Add page to favorites")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Add a page to the current user's favorites. Idempotent — re-adding an already-favorited page returns true.")
        .Produces<bool>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}