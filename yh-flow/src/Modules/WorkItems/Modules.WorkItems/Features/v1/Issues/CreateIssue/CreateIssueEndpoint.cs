using YH.Modules.WorkItems.Contracts.v1.Issues.CreateIssue;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using YH.Framework.Shared.Identity.Claims;

namespace YH.Modules.WorkItems.Features.v1.Issues.CreateIssue;

/// <summary>
/// POST /work-items/ — create an issue (REQ-4.3).
/// Requires workspace Admin, Member, or Guest role per CONTEXT.
/// Auto-assigns SequenceId and default state if not provided.
/// </summary>
public static class CreateIssueEndpoint
{
    internal static RouteHandlerBuilder MapCreateIssueEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/", async (Guid projectId, CreateIssueCommand command,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            command.ProjectId = projectId;
            var result = await mediator.Send(command, cancellationToken);
            return TypedResults.Created($"/api/v1/workspaces/{{slug}}/projects/{projectId}/work-items/{result.Id}", result);
        })
        .WithName("CreateIssue")
        .WithSummary("Create issue")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member, WorkspaceRole.Guest)
        .WithDescription("Create a new issue. SequenceId is auto-assigned. StateId is auto-assigned to project default if not provided. Supports M2M assignees and labels.")
        .Produces<CreateIssueResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
