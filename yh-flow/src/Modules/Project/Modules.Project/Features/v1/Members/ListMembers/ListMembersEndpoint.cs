using YH.Framework.Core.Exceptions;
using YH.Framework.Shared.Persistence;
using YH.Modules.Project.Contracts.DTOs;
using YH.Modules.Project.Contracts.v1.Members.ListMembers;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Project.Features.v1.Members.ListMembers;

/// <summary>
/// GET /api/v1/workspaces/{slug}/projects/{projectId}/members/ — list members of a project
/// (REQ-3.2). Workspace members and admins can browse the roster.
/// </summary>
/// <remarks>
/// <para>Decorated <c>.RequireWorkspaceRole(Member, Admin)</c> — workspace members and
/// admins may browse project member rosters; guests cannot.</para>
/// <para>The handler performs a two-phase batch user resolution (one Identity SQL per list call)
/// to avoid N+1 (RESEARCH Pitfall 3, NFR-1).</para>
/// </remarks>
public static class ListMembersEndpoint
{
    internal static RouteHandlerBuilder MapListMembersEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/", async (Guid projectId,
            HttpRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            if (projectId == Guid.Empty)
            {
                throw new NotFoundException("Project was not resolved.");
            }

            int? pageNumber = TryParseInt(request.Query["page"]);
            int? pageSize = TryParseInt(request.Query["per_page"]) ?? TryParseInt(request.Query["page_size"]);

            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}/api/v1/workspaces/{{slug}}/projects/{projectId}/members/";

            var result = await mediator.Send(new ListMembersQuery
            {
                ProjectId = projectId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                BaseUrl = baseUrl,
            }, cancellationToken);
            return TypedResults.Ok(result);
        })
        .WithName("ListProjectMembers")
        .WithSummary("List project members")
        .RequireAuthorization()
        .RequireWorkspaceRole(WorkspaceRole.Member, WorkspaceRole.Admin)
        .WithDescription("List members of a project. Plane-compatible paginated response with batch-resolved user details.")
        .Produces<PlanePagedResult<ProjectMemberDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }

    private static int? TryParseInt(string? value) =>
        int.TryParse(value, out var parsed) ? parsed : null;
}
