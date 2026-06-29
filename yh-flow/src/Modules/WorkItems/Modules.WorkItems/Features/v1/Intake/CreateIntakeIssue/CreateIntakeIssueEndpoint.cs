using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Intake.CreateIntakeIssue;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Intake.CreateIntakeIssue;

/// <summary>
/// POST /work-items/intake/ — create an intake issue (REQ-4.8).
/// Requires workspace Admin, Member, or Guest role.
/// Creates a draft Issue and IntakeIssue in two steps.
/// </summary>
public static class CreateIntakeIssueEndpoint
{
    internal static RouteHandlerBuilder MapCreateIntakeIssueEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/intake", async (Guid projectId, CreateIntakeIssueCommand command,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.ProjectId = projectId;
            var result = await mediator.Send(command, cancellationToken);
            return TypedResults.Created($"/api/v1/workspaces/{{slug}}/projects/{projectId}/work-items/intake/{result.Id}", result);
        })
        .WithName("CreateIntakeIssue")
        .WithSummary("Create intake issue")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member, WorkspaceRole.Guest)
        .WithDescription("Create a draft issue in the Intake inbox. Two-step: draft Issue (IsDraft=true) + IntakeIssue creation.")
        .Produces<IntakeIssueDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
