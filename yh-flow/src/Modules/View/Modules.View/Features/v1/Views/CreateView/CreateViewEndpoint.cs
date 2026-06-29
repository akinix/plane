using YH.Framework.Core.Exceptions;
using YH.Modules.View.Contracts.v1.Views.CreateView;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Framework.Shared.Identity.Claims;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.View.Features.v1.Views.CreateView;

/// <summary>
/// POST /api/v1/workspaces/{slug}/projects/{projectId}/views/ — create a view (REQ-8.1).
/// Requires workspace Admin or Member role.
/// </summary>
public static class CreateViewEndpoint
{
    internal static RouteHandlerBuilder MapCreateViewEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/", async (CreateViewCommand command,
            string slug,
            Guid projectId,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            command.ProjectId = projectId;
            command.OwnedBy = Guid.Parse(userId);
            var result = await mediator.Send(command, cancellationToken);
            return TypedResults.Created($"/api/v1/workspaces/{slug}/projects/{projectId}/views/{result.Id}", result);
        })
        .WithName("CreateView")
        .WithSummary("Create view")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Create a new view in the specified project. The authenticated user becomes the owner.")
        .Produces<CreateViewResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
