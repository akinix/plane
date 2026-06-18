using YH.Framework.Core.Exceptions;
using YH.Framework.Shared.Identity.Claims;
using YH.Modules.Workspace.Contracts.v1.Workspaces.ListUserWorkspaces;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.Workspace.Features.v1.Workspaces.ListUserWorkspaces;

/// <summary>
/// GET /api/v1/users/me/workspaces/ — list the current user's workspaces (REQ-2.1).
/// </summary>
/// <remarks>
/// Top-level endpoint (no <c>{slug}</c>); any authenticated user may list their own workspaces.
/// Mirrors Plane's <c>GET /api/v1/users/me/workspaces/</c>. Pagination uses
/// <c>?page=</c>/<c>per_page=</c> query strings.
/// </remarks>
public static class ListUserWorkspacesEndpoint
{
    internal static RouteHandlerBuilder MapListUserWorkspacesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/api/v{version:apiVersion}/users/me/workspaces/", async (HttpRequest request,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            // Plane-compatible pagination params.
            int? pageNumber = TryParseInt(request.Query["page"]);
            int? pageSize = TryParseInt(request.Query["per_page"]) ?? TryParseInt(request.Query["page_size"]);

            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}/api/v1/users/me/workspaces/";

            var result = await mediator.Send(new ListUserWorkspacesQuery
            {
                UserId = userId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                BaseUrl = baseUrl,
            }, cancellationToken);
            return TypedResults.Ok(result);
        })
        .WithName("ListUserWorkspaces")
        .WithSummary("List my workspaces")
        .RequireAuthorization()
        .WithDescription("List workspaces the current user is a member of. Plane-compatible paginated response.")
        .Produces<YH.Framework.Shared.Persistence.PlanePagedResult<YH.Modules.Workspace.Contracts.DTOs.WorkspaceDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }

    private static int? TryParseInt(string? value) =>
        int.TryParse(value, out var parsed) ? parsed : null;
}
