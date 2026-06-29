using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Modules.Links;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Modules.Links.ListModuleLinks;

/// <summary>
/// GET /modules/{moduleId}/links — list links in a module.
/// Any authenticated user may list module links.
/// </summary>
public static class ListModuleLinksEndpoint
{
    internal static RouteHandlerBuilder MapListModuleLinksEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{moduleId}/links", async (Guid projectId, Guid moduleId,
            IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new ListModuleLinksQuery
            {
                ProjectId = projectId,
                ModuleId = moduleId,
            }, cancellationToken)))
        .WithName("ListModuleLinks")
        .WithSummary("List module links")
        .RequireAuthorization()
        .WithDescription("List all external resource links in a module.")
        .Produces<List<ModuleLinkDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
