using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Modules.GetModule;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Modules.GetModule;

/// <summary>
/// GET /modules/{moduleId}/ — fetch a module by id.
/// Any authenticated user may read module metadata.
/// </summary>
public static class GetModuleEndpoint
{
    internal static RouteHandlerBuilder MapGetModuleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{moduleId}", async (Guid projectId, Guid moduleId,
            IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new GetModuleQuery
            {
                ProjectId = projectId,
                ModuleId = moduleId,
            }, cancellationToken)))
        .WithName("GetModule")
        .WithSummary("Get module by id")
        .RequireAuthorization()
        .WithDescription("Fetch a module by id. Any authenticated user may read module metadata.")
        .Produces<ModuleDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}
