using YH.Modules.WorkItems.Contracts.v1.Cycles.DateCheckCycle;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Cycles.DateCheckCycle;

/// <summary>
/// POST /cycles/date-check/ — check if dates overlap with existing cycles.
/// Requires workspace Admin or Member role.
/// </summary>
public static class DateCheckCycleEndpoint
{
    internal static RouteHandlerBuilder MapDateCheckCycleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/date-check", async (Guid projectId, DateCheckCycleCommand command,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.ProjectId = projectId;
            return TypedResults.Ok(await mediator.Send(command, cancellationToken));
        })
        .WithName("DateCheckCycle")
        .WithSummary("Check cycle date overlap")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Check if the given date range overlaps with any existing cycle in the project. Uses Plane-style triple overlap detection.")
        .Produces<DateCheckCycleResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
