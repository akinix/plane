using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.IssueLinks.CreateIssueLink;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.IssueLinks.CreateIssueLink;

/// <summary>
/// POST /work-items/{issueId}/links/ — create an issue link (REQ-4.4).
/// Requires workspace Admin or Member role.
/// Validates at least one of RelatedIssueId or Url is provided.
/// Validates RelatedIssueId exists in the same project.
/// </summary>
public static class CreateIssueLinkEndpoint
{
    internal static RouteHandlerBuilder MapCreateIssueLinkEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/{issueId}/links", async (Guid projectId, Guid issueId, CreateIssueLinkCommand command,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.IssueId = issueId;
            command.ProjectId = projectId;
            var result = await mediator.Send(command, cancellationToken);
            return TypedResults.Created($"/api/v1/workspaces/{{slug}}/projects/{projectId}/work-items/{issueId}/links/{result.Id}", result);
        })
        .WithName("CreateIssueLink")
        .WithSummary("Create issue link")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Create an issue link (external URL or internal Issue relation). At least one of RelatedIssueId or Url must be provided. Validates RelatedIssueId exists in the same project.")
        .Produces<IssueLinkDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
