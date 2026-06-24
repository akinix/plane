using YH.Modules.WorkItems.Contracts.v1.Modules.Links;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Modules.Links.RemoveModuleLink;

/// <summary>
/// DELETE /modules/{moduleId}/links/{linkId} — remove a link from a module.
/// Requires workspace Admin or Member role.
/// </summary>
public static class RemoveModuleLinkEndpoint
{
    internal static RouteHandlerBuilder MapRemoveModuleLinkEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/{moduleId}/links/{linkId}", async (Guid projectId, Guid moduleId, Guid linkId,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new RemoveModuleLinkCommand
            {
                ProjectId = projectId,
                ModuleId = moduleId,
                LinkId = linkId,
            }, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("RemoveModuleLink")
        .WithSummary("Remove link from module")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Soft-delete a module link, removing the link from the module.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
