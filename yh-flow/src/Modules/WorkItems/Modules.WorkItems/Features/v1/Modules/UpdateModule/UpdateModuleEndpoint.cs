using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Modules.UpdateModule;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Modules.UpdateModule;

/// <summary>
/// PATCH /modules/{moduleId}/ — update mutable module fields.
/// Requires workspace Admin or Member role.
/// </summary>
public static class UpdateModuleEndpoint
{
    private static readonly string[] HttpPatch = ["PATCH"];

    internal static RouteHandlerBuilder MapUpdateModuleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapMethods("/{moduleId}", HttpPatch,
            async (Guid projectId, Guid moduleId, UpdateModuleCommand command,
                IMediator mediator, CancellationToken cancellationToken) =>
            {
                command.ProjectId = projectId;
                command.ModuleId = moduleId;
                return TypedResults.Ok(await mediator.Send(command, cancellationToken));
            })
        .WithName("UpdateModule")
        .WithSummary("Update module")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Update mutable module fields (name, description, status, dates, lead, sort_order). All fields are optional (PATCH semantics).")
        .Produces<ModuleDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
