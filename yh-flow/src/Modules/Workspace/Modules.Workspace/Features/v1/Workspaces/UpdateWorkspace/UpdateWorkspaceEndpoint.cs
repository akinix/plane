using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Contracts.DTOs;
using YH.Modules.Workspace.Contracts.v1.Workspaces.UpdateWorkspace;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Workspace.Features.v1.Workspaces.UpdateWorkspace;

/// <summary>
/// PATCH /api/v1/workspaces/{slug} — update workspace settings (REQ-2.3).
/// </summary>
/// <remarks>
/// Decorated with <c>.RequireWorkspaceRole(WorkspaceRole.Admin)</c> — only Admins may edit
/// workspace settings (threat T-2-eop-update [BLOCKING] mitigation). Member/Guest requests return 403.
/// </remarks>
public static class UpdateWorkspaceEndpoint
{
    // Static readonly to avoid CA1861 (constant-array-argument); accepts both PATCH (Plane) and PUT.
    private static readonly string[] HttpPatchPut = { "PATCH", "PUT" };

    internal static RouteHandlerBuilder MapUpdateWorkspaceEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapMethods("/{slug}", HttpPatchPut,
            async (string slug, UpdateWorkspaceCommand command, IMediator mediator, CancellationToken cancellationToken) =>
            {
                command.Slug = slug;
                return TypedResults.Ok(await mediator.Send(command, cancellationToken));
            })
        .WithName("UpdateWorkspace")
        .WithSummary("Update workspace settings")
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("Update mutable workspace settings (name, logo, timezone, ...). Slug is not editable. Admin only.")
        .Produces<WorkspaceDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
