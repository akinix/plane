using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.IssueComments.CreateIssueComment;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Framework.Shared.Identity.Claims;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace YH.Modules.WorkItems.Features.v1.IssueComments.CreateIssueComment;

/// <summary>
/// POST /work-items/{issueId}/comments/ — create an issue comment (REQ-4.5).
/// Requires workspace Admin, Member, or Guest role.
/// ActorId is populated from the authenticated user.
/// </summary>
public static class CreateIssueCommentEndpoint
{
    internal static RouteHandlerBuilder MapCreateIssueCommentEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/{issueId}/comments", async (Guid projectId, Guid issueId, CreateIssueCommentCommand command,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            command.IssueId = issueId;
            command.ProjectId = projectId;
            command.ActorId = user.GetUserId() ?? throw new YH.Framework.Core.Exceptions.UnauthorizedException();
            var result = await mediator.Send(command, cancellationToken);
            return TypedResults.Created($"/api/v1/workspaces/{{slug}}/projects/{projectId}/work-items/{issueId}/comments/{result.Id}", result);
        })
        .WithName("CreateIssueComment")
        .WithSummary("Create issue comment")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member, WorkspaceRole.Guest)
        .WithDescription("Create a comment on an issue. HTML is sanitized. ParentId must reference an existing comment in the same issue.")
        .Produces<IssueCommentDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
