using YH.Modules.WorkItems.Contracts.v1.Cycles.Issues;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Cycles.Issues.AddIssuesToCycle;

/// <summary>
/// POST /cycles/{cycleId}/cycle-issues — add issues to a cycle.
/// Requires workspace Admin or Member role.
/// </summary>
public static class AddIssuesToCycleEndpoint
{
    internal static RouteHandlerBuilder MapAddIssuesToCycleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/{cycleId}/cycle-issues", async (Guid projectId, Guid cycleId,
            AddIssuesToCycleCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.ProjectId = projectId;
            command.CycleId = cycleId;
            await mediator.Send(command, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("AddIssuesToCycle")
        .WithSummary("Add issues to cycle")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Add issues to a cycle. The cycle must not be COMPLETED.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
