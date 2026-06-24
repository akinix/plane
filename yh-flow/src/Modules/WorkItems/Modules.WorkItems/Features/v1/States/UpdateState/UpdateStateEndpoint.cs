using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.States.UpdateState;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.States.UpdateState;

/// <summary>
/// PATCH /states/{stateId}/ — update mutable state display fields (REQ-4.1).
/// Requires workspace Admin or Member role.
/// </summary>
public static class UpdateStateEndpoint
{
    private static readonly string[] HttpPatch = ["PATCH"];

    internal static RouteHandlerBuilder MapUpdateStateEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapMethods("/{stateId}", HttpPatch,
            async (Guid projectId, Guid stateId, UpdateStateCommand command,
                IMediator mediator, CancellationToken cancellationToken) =>
            {
                command.ProjectId = projectId;
                command.StateId = stateId;
                return TypedResults.Ok(await mediator.Send(command, cancellationToken));
            })
        .WithName("UpdateState")
        .WithSummary("Update state")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Update mutable state display fields (name, color, sort_order). All fields are optional (PATCH semantics).")
        .Produces<StateDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
