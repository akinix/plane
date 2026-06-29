using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Labels.UpdateLabel;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Labels.UpdateLabel;

/// <summary>
/// PATCH /labels/{labelId}/ — update mutable label fields (REQ-4.2).
/// Requires workspace Admin role per CONTEXT.
/// </summary>
public static class UpdateLabelEndpoint
{
    private static readonly string[] HttpPatch = ["PATCH"];

    internal static RouteHandlerBuilder MapUpdateLabelEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapMethods("/{labelId}", HttpPatch,
            async (Guid projectId, Guid labelId, UpdateLabelCommand command,
                IMediator mediator, CancellationToken cancellationToken) =>
            {
                command.ProjectId = projectId;
                command.LabelId = labelId;
                return TypedResults.Ok(await mediator.Send(command, cancellationToken));
            })
        .WithName("UpdateLabel")
        .WithSummary("Update label")
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("Update mutable label fields (name, color, parentId, description, sort_order). All fields are optional (PATCH semantics).")
        .Produces<LabelDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
