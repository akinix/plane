using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Cycles.UpdateCycle;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Cycles.UpdateCycle;

/// <summary>
/// PATCH /cycles/{cycleId}/ — update mutable cycle fields.
/// Requires workspace Admin or Member role.
/// </summary>
public static class UpdateCycleEndpoint
{
    private static readonly string[] HttpPatch = ["PATCH"];

    internal static RouteHandlerBuilder MapUpdateCycleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapMethods("/{cycleId}", HttpPatch,
            async (Guid projectId, Guid cycleId, UpdateCycleCommand command,
                IMediator mediator, CancellationToken cancellationToken) =>
            {
                command.ProjectId = projectId;
                command.CycleId = cycleId;
                return TypedResults.Ok(await mediator.Send(command, cancellationToken));
            })
        .WithName("UpdateCycle")
        .WithSummary("Update cycle")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Update mutable cycle fields (name, description, dates, sort_order, timezone). All fields are optional (PATCH semantics).")
        .Produces<CycleDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
