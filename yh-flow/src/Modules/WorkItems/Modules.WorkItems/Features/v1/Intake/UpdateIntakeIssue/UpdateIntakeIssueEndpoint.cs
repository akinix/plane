using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Intake.UpdateIntakeIssue;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Intake.UpdateIntakeIssue;

/// <summary>
/// PATCH /work-items/intake/{intakeIssueId}/ — update intake issue status (REQ-4.8).
/// Accept requires Admin or Member role (elevated permission — T-4-intake-01).
/// Reject/Snooze/Duplicate require Admin or Member role.
/// </summary>
public static class UpdateIntakeIssueEndpoint
{
    internal static RouteHandlerBuilder MapUpdateIntakeIssueEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPatch("/intake/{intakeIssueId}", async (Guid projectId, Guid intakeIssueId,
            UpdateIntakeIssueCommand command,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.IntakeIssueId = intakeIssueId;
            command.ProjectId = projectId;
            return TypedResults.Ok(await mediator.Send(command, cancellationToken));
        })
        .WithName("UpdateIntakeIssue")
        .WithSummary("Update intake issue status")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Transition intake issue status. Accept converts the draft Issue to an active Issue with default state. Reject/Snooze/Duplicate are also supported.")
        .Produces<IntakeIssueDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
