using YH.Modules.WorkItems.Contracts.v1.Modules.ArchiveModule;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Modules.UnarchiveModule;

/// <summary>
/// DELETE /archived-modules/{moduleId}/ — unarchive a module (restore to active).
/// Requires workspace Admin or Member role.
/// This endpoint is registered on the <c>archived-modules</c> route group.
/// </summary>
public static class UnarchiveModuleEndpoint
{
    private static readonly string[] HttpDelete = ["DELETE"];

    internal static RouteHandlerBuilder MapUnarchiveModuleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapMethods("/{moduleId}", HttpDelete,
            async (Guid projectId, Guid moduleId,
                IMediator mediator, CancellationToken cancellationToken) =>
            {
                await mediator.Send(new UnarchiveModuleCommand
                {
                    ProjectId = projectId,
                    ModuleId = moduleId,
                }, cancellationToken);
                return TypedResults.NoContent();
            })
        .WithName("UnarchiveModule")
        .WithSummary("Unarchive module")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Unarchive a module, restoring it to active status.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
