using YH.Framework.Core.Exceptions;
using YH.Modules.Page.Contracts.v1.Pages.CreatePage;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Framework.Shared.Identity.Claims;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.Page.Features.v1.Pages.CreatePage;

/// <summary>
/// POST /api/v1/workspaces/{slug}/projects/{projectId}/pages/ — create a page (REQ-7.1).
/// Requires workspace Admin or Member role.
/// </summary>
public static class CreatePageEndpoint
{
    internal static RouteHandlerBuilder MapCreatePageEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/", async (CreatePageCommand command,
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
            return TypedResults.Created($"/api/v1/workspaces/{{slug}}/projects/{projectId}/pages/{result.Id}", result);
        })
        .WithName("CreatePage")
        .WithSummary("Create page")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Create a new page in the specified project. The authenticated user becomes the owner.")
        .Produces<CreatePageResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}