using YH.Modules.WorkItems.Contracts.v1.Cycles.Issues;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Cycles.Issues.RemoveIssueFromCycle;

/// <summary>
/// DELETE /cycles/{cycleId}/cycle-issues/{issueId} — remove an issue from a cycle.
/// Requires workspace Admin or Member role.
/// </summary>
public static class RemoveIssueFromCycleEndpoint
{
    internal static RouteHandlerBuilder MapRemoveIssueFromCycleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{cycleId}/cycle-issues/{issueId}", async (Guid projectId, Guid cycleId, Guid issueId,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new RemoveIssueFromCycleCommand
            {
                ProjectId = projectId,
                CycleId = cycleId,
                IssueId = issueId,
            }, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("RemoveIssueFromCycle")
        .WithSummary("Remove issue from cycle")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Soft-delete a cycle-issue association, removing the issue from the cycle.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
