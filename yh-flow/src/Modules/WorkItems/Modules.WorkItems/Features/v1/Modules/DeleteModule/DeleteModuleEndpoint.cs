using YH.Modules.WorkItems.Contracts.v1.Modules.DeleteModule;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Modules.DeleteModule;

/// <summary>
/// DELETE /modules/{moduleId}/ — soft-delete a module.
/// Requires workspace Admin or Member role.
/// </summary>
public static class DeleteModuleEndpoint
{
    internal static RouteHandlerBuilder MapDeleteModuleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{moduleId}", async (Guid projectId, Guid moduleId,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteModuleCommand
            {
                ProjectId = projectId,
                ModuleId = moduleId,
            }, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("DeleteModule")
        .WithSummary("Delete module")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Soft-delete a module.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
