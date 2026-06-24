using YH.Framework.Core.Exceptions;
using YH.Modules.Project.Contracts.v1.Members.AddMember;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Framework.Shared.Identity.Claims;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.Project.Features.v1.Members.AddMember;

/// <summary>
/// POST /api/v1/workspaces/{slug}/projects/{projectId}/members/ — add a member to a project
/// (REQ-3.2). Workspace Admin or project Admin required.
/// </summary>
/// <remarks>
/// The endpoint enforces workspace Admin via <c>.RequireWorkspaceRole(Admin)</c>; the handler
/// additionally checks project-level Admin role for defense-in-depth.
/// </remarks>
public static class AddMemberEndpoint
{
    internal static RouteHandlerBuilder MapAddMemberEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/", async (Guid projectId,
            AddMemberCommand command,
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
            command.CurrentUserId = Guid.Parse(userId);
            var result = await mediator.Send(command, cancellationToken);
            return TypedResults.Created($"/api/v1/workspaces/{{slug}}/projects/{projectId}/members/{result.Id}", result);
        })
        .WithName("AddProjectMember")
        .WithSummary("Add project member")
        .RequireAuthorization()
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("Add a user as a project member. Workspace Admin or project Admin required.")
        .Produces<AddMemberResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
