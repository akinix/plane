using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Issues.GetIssue;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Issues.GetIssue;

/// <summary>
/// GET /work-items/{issueId}/ — fetch an issue by id with detail fields (REQ-4.3).
/// Any authenticated user may read issue details.
/// </summary>
public static class GetIssueEndpoint
{
    internal static RouteHandlerBuilder MapGetIssueEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{issueId}", async (Guid projectId, Guid issueId,
            IMediator mediator, CancellationToken cancellationToken) =>
            TypedResults.Ok(await mediator.Send(new GetIssueQuery
            {
                ProjectId = projectId,
                IssueId = issueId,
            }, cancellationToken)))
        .WithName("GetIssue")
        .WithSummary("Get issue by id")
        .RequireAuthorization()
        .WithDescription("Fetch an issue by id with assignees, labels, state info, and sequence id display.")
        .Produces<IssueDetailDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}
