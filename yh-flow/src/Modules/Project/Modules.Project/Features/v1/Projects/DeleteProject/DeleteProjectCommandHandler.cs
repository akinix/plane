using System.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Project.Contracts.v1.Projects.DeleteProject;
using YH.Modules.Project.Data;
using YH.Modules.Project.Domain;
using YH.Modules.Workspace.Contracts;
using ProjectEntity = YH.Modules.Project.Domain.Project;

namespace YH.Modules.Project.Features.v1.Projects.DeleteProject;

/// <summary>
/// Handles <see cref="DeleteProjectCommand"/> — soft-deletes a project, releasing its slug
/// for reuse (D-03 pattern: appends <c>__{epoch}</c> to slug).
/// </summary>
/// <remarks>
/// <b>Authorization (dual-gate per D-09):</b> the endpoint enforces workspace Admin via
/// <c>RequireWorkspaceRole(WorkspaceRole.Admin)</c>. The handler additionally verifies
/// project-level Admin role via <see cref="ProjectMember"/> table.
/// </remarks>
public sealed class DeleteProjectCommandHandler : ICommandHandler<DeleteProjectCommand>
{
    private readonly ProjectDbContext _db;

    public DeleteProjectCommandHandler(ProjectDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(DeleteProjectCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ProjectId == Guid.Empty)
        {
            throw new CustomException("Project id is required.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        if (command.CurrentUserId == Guid.Empty)
        {
            throw new CustomException("Current user id is required.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        // Fetch existing project (tenant-scoped by DbContext).
        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId && !p.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (project is null)
        {
            throw new NotFoundException($"Project '{command.ProjectId}' was not found.");
        }

        // Authorization (dual-gate per D-09): verify project-level Admin role.
        var currentUserId = command.CurrentUserId.ToString();
        var isProjectAdmin = await _db.Members
            .AnyAsync(m => m.ProjectId == project.Id
                        && m.UserId == currentUserId
                        && m.Role >= (int)WorkspaceRole.Admin
                        && m.IsActive
                        && !m.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (!isProjectAdmin)
        {
            throw new CustomException(
                "You do not have permission to delete this project.",
                Array.Empty<string>(),
                HttpStatusCode.Forbidden);
        }

        // D-03: soft delete with slug epoch release.
        project.SoftDelete(DateTimeOffset.UtcNow);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
