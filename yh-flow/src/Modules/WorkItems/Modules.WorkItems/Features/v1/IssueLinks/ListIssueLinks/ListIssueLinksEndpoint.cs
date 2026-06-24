using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.IssueLinks.ListIssueLinks;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.IssueLinks.ListIssueLinks;

/// <summary>
/// GET /work-items/{issueId}/links/ — list all links for an issue (REQ-4.4).
/// Any authenticated user may list links.
/// </summary>
public static class ListIssueLinksEndpoint
{
    internal static RouteHandlerBuilder MapListIssueLinksEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{issueId}/links", async (Guid issueId,
            IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new ListIssueLinksQuery
            {
                IssueId = issueId,
            }, cancellationToken)))
        .WithName("ListIssueLinks")
        .WithSummary("List issue links")
        .RequireAuthorization()
        .WithDescription("List all links for an issue (external URLs and internal relations).")
        .Produces<List<IssueLinkDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
