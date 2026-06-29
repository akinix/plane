using YH.Modules.WorkItems.Contracts.v1.Modules.Issues;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Modules.Issues.RemoveIssueFromModule;

/// <summary>
/// DELETE /modules/{moduleId}/module-issues/{issueId} — remove an issue from a module.
/// Requires workspace Admin or Member role.
/// </summary>
public static class RemoveIssueFromModuleEndpoint
{
    internal static RouteHandlerBuilder MapRemoveIssueFromModuleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{moduleId}/module-issues/{issueId}", async (Guid projectId, Guid moduleId, Guid issueId,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new RemoveIssueFromModuleCommand
            {
                ProjectId = projectId,
                ModuleId = moduleId,
                IssueId = issueId,
            }, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("RemoveIssueFromModule")
        .WithSummary("Remove issue from module")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Soft-delete a module-issue association, removing the issue from the module.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
