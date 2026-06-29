using YH.Modules.WorkItems.Contracts.v1.Cycles.ArchiveCycle;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Cycles.UnarchiveCycle;

/// <summary>
/// DELETE /archived-cycles/{cycleId}/ — unarchive a cycle (restore to active).
/// Requires workspace Admin or Member role.
/// This endpoint is registered on the <c>archived-cycles</c> route group.
/// </summary>
public static class UnarchiveCycleEndpoint
{
    private static readonly string[] HttpDelete = ["DELETE"];

    internal static RouteHandlerBuilder MapUnarchiveCycleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapMethods("/{cycleId}", HttpDelete,
            async (Guid projectId, Guid cycleId,
                IMediator mediator, CancellationToken cancellationToken) =>
            {
                await mediator.Send(new UnarchiveCycleCommand
                {
                    ProjectId = projectId,
                    CycleId = cycleId,
                }, cancellationToken);
                return TypedResults.NoContent();
            })
        .WithName("UnarchiveCycle")
        .WithSummary("Unarchive cycle")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Unarchive a cycle, restoring it to active status.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
