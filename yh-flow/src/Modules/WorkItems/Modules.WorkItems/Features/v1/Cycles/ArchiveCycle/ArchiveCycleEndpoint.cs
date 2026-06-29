using YH.Modules.WorkItems.Contracts.v1.Cycles.ArchiveCycle;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Cycles.ArchiveCycle;

/// <summary>
/// POST /cycles/{cycleId}/archive — archive a completed cycle.
/// Requires workspace Admin or Member role.
/// </summary>
public static class ArchiveCycleEndpoint
{
    internal static RouteHandlerBuilder MapArchiveCycleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/{cycleId}/archive", async (Guid projectId, Guid cycleId,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new ArchiveCycleCommand
            {
                ProjectId = projectId,
                CycleId = cycleId,
            }, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("ArchiveCycle")
        .WithSummary("Archive cycle")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Archive a completed cycle. Only cycles with EndDate in the past can be archived.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
