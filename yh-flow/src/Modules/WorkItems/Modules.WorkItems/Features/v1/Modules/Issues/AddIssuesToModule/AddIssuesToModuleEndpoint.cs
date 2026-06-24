using YH.Modules.WorkItems.Contracts.v1.Modules.Issues;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Modules.Issues.AddIssuesToModule;

/// <summary>
/// POST /modules/{moduleId}/module-issues — add issues to a module.
/// Requires workspace Admin or Member role.
/// </summary>
public static class AddIssuesToModuleEndpoint
{
    internal static RouteHandlerBuilder MapAddIssuesToModuleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/{moduleId}/module-issues", async (Guid projectId, Guid moduleId,
            AddIssuesToModuleCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.ProjectId = projectId;
            command.ModuleId = moduleId;
            await mediator.Send(command, cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("AddIssuesToModule")
        .WithSummary("Add issues to module")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Add issues to a module. Module has no COMPLETED restriction — issues can be added at any time.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
