using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;

namespace YH.Modules.Analytics.Features.v1.Export.ExportAnalytics;

/// <summary>
/// Request body for export endpoint.
/// </summary>
public sealed record ExportAnalyticsRequest
{
    /// <summary>Optional comma-separated project IDs to filter export.</summary>
    public string? ProjectIds { get; init; }
}

/// <summary>
/// POST /api/v1/workspaces/{slug}/analytics/export/ — triggers async CSV export via Hangfire.
/// Requires workspace Admin or Member role.
/// </summary>
public static class ExportAnalyticsEndpoint
{
    internal static RouteHandlerBuilder MapExportAnalyticsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/export", async (
            string slug,
            [Microsoft.AspNetCore.Mvc.FromBody]
            ExportAnalyticsRequest? request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            return await mediator.Send(
                new ExportAnalyticsCommand(slug, request?.ProjectIds), ct);
        })
        .WithName("ExportAnalytics")
        .WithSummary("Trigger analytics CSV export")
        .WithDescription("Enqueues a Hangfire background job that generates a CSV export of analytics data. The job writes the CSV file to the server's temp directory.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member);
    }
}
