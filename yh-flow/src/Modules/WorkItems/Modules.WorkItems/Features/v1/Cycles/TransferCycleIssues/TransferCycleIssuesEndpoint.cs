using YH.Modules.WorkItems.Contracts.v1.Cycles.TransferCycleIssues;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Cycles.TransferCycleIssues;

/// <summary>
/// POST /cycles/{cycleId}/transfer-issues — transfer uncompleted issues to another cycle.
/// Requires workspace Admin or Member role.
/// </summary>
public static class TransferCycleIssuesEndpoint
{
    internal static RouteHandlerBuilder MapTransferCycleIssuesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/{cycleId}/transfer-issues", async (Guid projectId, Guid cycleId,
            TransferCycleIssuesCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.ProjectId = projectId;
            command.CycleId = cycleId;
            await mediator.Send(command, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("TransferCycleIssues")
        .WithSummary("Transfer cycle issues")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Transfer uncompleted issues from the current cycle to another cycle. Builds a progress snapshot on the source cycle before migration.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
