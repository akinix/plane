using System.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Workspace.Contracts.DTOs;
using YH.Modules.Workspace.Contracts.v1.Workspaces.UpdateWorkspace;
using YH.Modules.Workspace.Data;

namespace YH.Modules.Workspace.Features.v1.Workspaces.UpdateWorkspace;

/// <summary>
/// Handles <see cref="UpdateWorkspaceCommand"/> — applies PATCH updates to mutable display fields
/// (REQ-2.3 settings). Slug is NOT editable here.
/// </summary>
/// <remarks>
/// <b>Cache invalidation (T-2-cacheinvalid):</b> not required — Update does not change the slug
/// (see remarks on the <see cref="Domain.Workspace.Update"/> method), so the cached slug→tenant
/// resolution stays valid. If slug transfer is added later (Phase 3+) the handler MUST call
/// <c>IMultiTenantStore&lt;AppTenantInfo&gt;.UpdateAsync(...)</c> or <c>RemoveAsync(slug)</c>.
/// </remarks>
public sealed class UpdateWorkspaceCommandHandler : IQueryHandler<UpdateWorkspaceCommand, WorkspaceDto>
{
    private readonly WorkspaceDbContext _db;

    public UpdateWorkspaceCommandHandler(WorkspaceDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<WorkspaceDto> Handle(UpdateWorkspaceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.Slug))
        {
            throw new CustomException("Slug is required.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        // Workspace is IGlobalEntity (no tenant filter); explicitly exclude soft-deleted rows.
        var ws = await _db.Workspaces
            .FirstOrDefaultAsync(w => w.Slug == command.Slug && !w.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (ws is null)
        {
            throw new NotFoundException($"Workspace '{command.Slug}' was not found.");
        }

        ws.Update(
            name: command.Name,
            logo: command.Logo,
            organizationSize: command.OrganizationSize,
            timeZone: command.TimeZone,
            backgroundColor: command.BackgroundColor);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return WorkspaceDtoMapper.ToDto(ws);
    }
}
