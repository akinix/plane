using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Modules.ListModules;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Modules.ListModules;

/// <summary>
/// GET /modules/ — list modules in a project.
/// Any authenticated user may list modules.
/// </summary>
public static class ListModulesEndpoint
{
    internal static RouteHandlerBuilder MapListModulesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/", async (Guid projectId, string? status,
            IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new ListModulesQuery
            {
                ProjectId = projectId,
                Status = status,
            }, cancellationToken)))
        .WithName("ListModules")
        .WithSummary("List modules")
        .RequireAuthorization()
        .WithDescription("List modules in a project, ordered by SortOrder ascending. Optional status filter: backlog, planned, in-progress, paused, completed, cancelled, all.")
        .Produces<List<ModuleDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
