using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Export.ExportIssues;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.ImportExport.ExportIssues;

/// <summary>
/// POST /work-items/export-issues/ — export issues as CSV or JSON (REQ-4.9).
/// Requires workspace Admin or Member role.
/// Returns a file download with the exported content.
/// </summary>
public static class ExportIssuesEndpoint
{
    internal static RouteHandlerBuilder MapExportIssuesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/export-issues", async (Guid projectId, ExportIssuesCommand command,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.ProjectId = projectId;
            var result = await mediator.Send(command, cancellationToken);
            return Results.File(
                result.FileContent,
                result.ContentType,
                result.FileName);
        })
        .WithName("ExportIssues")
        .WithSummary("Export issues")
        .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
        .WithDescription("Export issues as CSV or JSON. Supports optional state and priority filters. CSV output includes formula injection protection.")
        .Produces<ExportResultDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
