using YH.Framework.Shared.Persistence;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Issues.ListIssues;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.Issues.ListIssues;

/// <summary>
/// GET /work-items/ — list issues with multidimensional filtering and Plane-compatible pagination (REQ-4.3).
/// Any authenticated user may list issues.
/// </summary>
public static class ListIssuesEndpoint
{
    internal static RouteHandlerBuilder MapListIssuesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/", async (Guid projectId,
            int? pageNumber, int? pageSize, string? orderBy, string? baseUrl,
            Guid? stateId, string? priority, string? assigneeId, Guid? labelId, Guid? parentId, bool? isDraft,
            IMediator mediator, HttpRequest request, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new ListIssuesQuery
            {
                ProjectId = projectId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                OrderBy = orderBy,
                BaseUrl = baseUrl ?? $"{request.Scheme}://{request.Host}{request.Path}",
                StateId = stateId,
                Priority = priority,
                AssigneeId = assigneeId,
                LabelId = labelId,
                ParentId = parentId,
                IsDraft = isDraft,
            }, cancellationToken);

            return TypedResults.Ok(result);
        })
        .WithName("ListIssues")
        .WithSummary("List issues")
        .RequireAuthorization()
        .WithDescription("List issues with multidimensional filtering (state/priority/assignee/label/parent) and Plane-compatible pagination.")
        .Produces<PlanePagedResult<IssueDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
