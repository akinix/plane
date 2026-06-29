using YH.Modules.WorkItems.Contracts.v1.IssueLinks.DeleteIssueLink;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.IssueLinks.DeleteIssueLink;

/// <summary>
/// DELETE /work-items/{issueId}/links/{linkId}/ — hard-delete an issue link (REQ-4.4).
/// Requires workspace Admin role.
/// </summary>
public static class DeleteIssueLinkEndpoint
{
    internal static RouteHandlerBuilder MapDeleteIssueLinkEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{issueId}/links/{linkId}", async (Guid linkId,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteIssueLinkCommand
            {
                LinkId = linkId,
            }, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("DeleteIssueLink")
        .WithSummary("Delete issue link")
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("Hard-delete an issue link. Only workspace Admins may delete.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
