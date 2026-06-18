using YH.Framework.Core.Context;
using YH.Framework.Core.Exceptions;
using YH.Framework.Shared.Persistence;
using YH.Modules.Workspace;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Contracts.DTOs;
using YH.Modules.Workspace.Contracts.v1.Invitations.ListInvitations;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Workspace.Features.v1.Invitations.ListInvitations;

/// <summary>
/// GET /api/v1/workspaces/{slug}/invitations/ — list invitations for the resolved workspace
/// (REQ-2.4). Admin-only. Plane-compatible paginated response; raw tokens are NEVER serialised.
/// </summary>
public static class ListInvitationsEndpoint
{
    internal static RouteHandlerBuilder MapListInvitationsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/", async (
            HttpRequest request,
            ICurrentWorkspaceContext workspaceContext,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var workspaceId = workspaceContext.CurrentWorkspaceId
                ?? throw new NotFoundException("Workspace was not resolved.");

            int? pageNumber = TryParseInt(request.Query["page"]);
            int? pageSize = TryParseInt(request.Query["per_page"]) ?? TryParseInt(request.Query["page_size"]);
            bool pendingOnly = bool.TryParse(request.Query["pending"], out var p) && p;

            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}/api/v1/workspaces/{workspaceContext.Slug}/invitations/";

            var result = await mediator.Send(new ListInvitationsQuery
            {
                WorkspaceId = workspaceId,
                PendingOnly = pendingOnly,
                PageNumber = pageNumber,
                PageSize = pageSize,
                BaseUrl = baseUrl,
            }, cancellationToken);
            return TypedResults.Ok(result);
        })
        .WithName("ListWorkspaceInvitations")
        .WithSummary("List workspace invitations")
        .RequireAuthorization()
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("List invitations for the resolved workspace. Admin-only. Raw tokens are never returned.")
        .Produces<PlanePagedResult<WorkspaceInvitationDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }

    private static int? TryParseInt(string? value) =>
        int.TryParse(value, out var parsed) ? parsed : null;
}
