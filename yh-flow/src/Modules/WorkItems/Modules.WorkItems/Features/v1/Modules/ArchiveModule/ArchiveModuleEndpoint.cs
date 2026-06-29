using YH.Modules.WorkItems.Contracts.v1.Modules.ArchiveModule;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Modules.ArchiveModule;

/// <summary>
/// POST /modules/{moduleId}/archive — archive a module.
/// Requires workspace Admin or Member role.
/// Module has no date restrictions (unlike Cycle — any status can be archived).
/// </summary>
public static class ArchiveModuleEndpoint
{
    internal static RouteHandlerBuilder MapArchiveModuleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/{moduleId}/archive", async (Guid projectId, Guid moduleId,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new ArchiveModuleCommand
            {
                ProjectId = projectId,
                ModuleId = moduleId,
            }, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("ArchiveModule")
        .WithSummary("Archive module")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Archive a module. Module has no date restrictions — any status can be archived (unlike Cycle).")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
