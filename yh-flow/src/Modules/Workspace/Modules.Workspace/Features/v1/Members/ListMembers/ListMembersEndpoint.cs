using YH.Framework.Core.Context;
using YH.Framework.Core.Exceptions;
using YH.Framework.Shared.Persistence;
using YH.Modules.Workspace;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Contracts.DTOs;
using YH.Modules.Workspace.Contracts.v1.Members.ListMembers;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Workspace.Features.v1.Members.ListMembers;

/// <summary>
/// GET /api/v1/workspaces/{slug}/members/ — list members of the resolved workspace (REQ-2.2).
/// </summary>
/// <remarks>
/// Workspace-scoped route. Decorated <c>.RequireWorkspaceRole(Member, Admin)</c> — members and
/// admins may browse the roster; guests cannot. The handler performs the D-05 two-phase batch
/// user resolution (one Identity SQL per list call) to avoid N+1.
/// </remarks>
public static class ListMembersEndpoint
{
    internal static RouteHandlerBuilder MapListMembersEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/", async (HttpRequest request,
            ICurrentWorkspaceContext workspaceContext,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var workspaceId = workspaceContext.CurrentWorkspaceId
                ?? throw new NotFoundException("Workspace was not resolved.");

            int? pageNumber = TryParseInt(request.Query["page"]);
            int? pageSize = TryParseInt(request.Query["per_page"]) ?? TryParseInt(request.Query["page_size"]);

            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}/api/v1/workspaces/{workspaceContext.Slug}/members/";

            var result = await mediator.Send(new ListMembersQuery
            {
                WorkspaceId = workspaceId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                BaseUrl = baseUrl,
            }, cancellationToken);
            return TypedResults.Ok(result);
        })
        .WithName("ListWorkspaceMembers")
        .WithSummary("List workspace members")
        .RequireAuthorization()
        .RequireWorkspaceRole(WorkspaceRole.Member, WorkspaceRole.Admin)
        .WithDescription("List members of the resolved workspace. Plane-compatible paginated response.")
        .Produces<PlanePagedResult<WorkspaceMemberDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }

    private static int? TryParseInt(string? value) =>
        int.TryParse(value, out var parsed) ? parsed : null;
}
