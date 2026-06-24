using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Modules.Issues;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Modules.Issues.ListModuleIssues;

/// <summary>
/// GET /modules/{moduleId}/module-issues — list issues in a module.
/// Any authenticated user may list module issues.
/// </summary>
public static class ListModuleIssuesEndpoint
{
    internal static RouteHandlerBuilder MapListModuleIssuesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{moduleId}/module-issues", async (Guid projectId, Guid moduleId,
            IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new ListModuleIssuesQuery
            {
                ProjectId = projectId,
                ModuleId = moduleId,
            }, cancellationToken)))
        .WithName("ListModuleIssues")
        .WithSummary("List module issues")
        .RequireAuthorization()
        .WithDescription("List all issues in a module.")
        .Produces<List<IssueDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
