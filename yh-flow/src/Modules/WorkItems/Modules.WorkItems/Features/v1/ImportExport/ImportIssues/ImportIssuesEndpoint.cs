using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Import.ImportIssues;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.WorkItems.Features.v1.ImportExport.ImportIssues;

/// <summary>
/// POST /work-items/import-issues/ — import issues from CSV or JSON (REQ-4.9).
/// Requires workspace Admin role.
/// Accepts file upload (multipart/form-data) or raw JSON body.
/// </summary>
public static class ImportIssuesEndpoint
{
    internal static RouteHandlerBuilder MapImportIssuesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/import-issues", async (Guid projectId, ImportIssuesCommand command,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.ProjectId = projectId;
            var result = await mediator.Send(command, cancellationToken);
            return TypedResults.Ok(result);
        })
        .WithName("ImportIssues")
        .WithSummary("Import issues")
        .RequireWorkspaceRole(WorkspaceRole.Admin)
        .WithDescription("Import issues from CSV or JSON. Validates each row and reports per-row errors. Max file size: 10MB.")
        .Produces<ImportResultDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
