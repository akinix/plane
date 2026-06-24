using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Modules.ArchiveModule;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Modules.ListArchivedModules;

/// <summary>
/// GET /archived-modules/ — list archived modules in a project.
/// Any authenticated user may list archived modules.
/// This endpoint is registered on the <c>archived-modules</c> route group.
/// </summary>
public static class ListArchivedModulesEndpoint
{
    internal static RouteHandlerBuilder MapListArchivedModulesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/", async (Guid projectId,
            IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new ListArchivedModulesQuery
            {
                ProjectId = projectId,
            }, cancellationToken)))
        .WithName("ListArchivedModules")
        .WithSummary("List archived modules")
        .RequireAuthorization()
        .WithDescription("List archived modules in a project, ordered by archived date descending.")
        .Produces<List<ModuleDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
