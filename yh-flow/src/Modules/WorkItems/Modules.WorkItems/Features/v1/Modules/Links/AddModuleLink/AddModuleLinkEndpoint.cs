using YH.Modules.WorkItems.Contracts.v1.Modules.Links;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Modules.Links.AddModuleLink;

/// <summary>
/// POST /modules/{moduleId}/links — add a link to a module.
/// Requires workspace Admin or Member role.
/// </summary>
public static class AddModuleLinkEndpoint
{
    internal static RouteHandlerBuilder MapAddModuleLinkEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/{moduleId}/links", async (Guid projectId, Guid moduleId,
            AddModuleLinkCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.ProjectId = projectId;
            command.ModuleId = moduleId;
            await mediator.Send(command, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("AddModuleLink")
        .WithSummary("Add link to module")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Add an external resource link (e.g. Figma design, document) to a module.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
