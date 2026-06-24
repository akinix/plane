using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Context;
using YH.Framework.Core.Exceptions;
using YH.Modules.Project.Contracts;
using YH.Modules.Project.Contracts.DTOs;
using YH.Modules.Project.Contracts.v1.Projects.UpdateProject;
using YH.Modules.Project.Data;
using YH.Modules.Project.Domain;
using YH.Modules.Workspace.Contracts;
using ProjectEntity = YH.Modules.Project.Domain.Project;

namespace YH.Modules.Project.Features.v1.Projects.UpdateProject;

/// <summary>
/// Handles <see cref="UpdateProjectCommand"/> — applies PATCH updates to mutable project display fields
/// (REQ-3.1 / REQ-3.3 settings). Slug and Identifier are NOT editable here.
/// </summary>
/// <remarks>
/// <b>Authorization (dual-gate per D-09):</b> the endpoint enforces workspace Admin/Member via
/// <c>RequireWorkspaceRole</c>. The handler additionally verifies that workspace Members hold an
/// Admin role on the project (via <see cref="ProjectMember"/> table). Workspace Admins bypass the
/// project-level check.
/// </remarks>
public sealed class UpdateProjectCommandHandler : ICommandHandler<UpdateProjectCommand, ProjectDto>
{
    private readonly ProjectDbContext _db;
    private readonly ICurrentWorkspaceContext _workspaceContext;
    private readonly ICurrentUser _currentUser;

    public UpdateProjectCommandHandler(
        ProjectDbContext db,
        ICurrentWorkspaceContext workspaceContext,
        ICurrentUser currentUser)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _workspaceContext = workspaceContext ?? throw new ArgumentNullException(nameof(workspaceContext));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async ValueTask<ProjectDto> Handle(UpdateProjectCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ProjectId == Guid.Empty)
        {
            throw new CustomException("Project id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        // Fetch existing project (tenant-scoped by DbContext).
        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId && !p.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (project is null)
        {
            throw new NotFoundException($"Project '{command.ProjectId}' was not found.");
        }

        // Authorization (dual-gate): workspace Admins bypass project-level role check.
        var currentRole = _workspaceContext.CurrentUserRole;
        if (currentRole != WorkspaceRole.Admin)
        {
            // Workspace Members must hold an Admin role on the project.
            var currentUserId = _currentUser.GetUserId().ToString();
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
                    "You do not have permission to update this project.",
                    Array.Empty<string>(),
                    System.Net.HttpStatusCode.Forbidden);
            }
        }

        project.Update(
            name: command.Name,
            description: command.Description,
            descriptionText: command.DescriptionText,
            descriptionHtml: command.DescriptionHtml,
            network: command.Network.HasValue ? (ProjectNetwork)command.Network.Value : null,
            emoji: command.Emoji,
            iconProp: command.IconProp,
            coverImageUrl: command.CoverImageUrl,
            logoProps: command.LogoProps,
            timeZone: command.TimeZone,
            projectLeadId: command.ProjectLeadId,
            defaultAssigneeId: command.DefaultAssigneeId,
            moduleViewEnabled: command.ModuleViewEnabled,
            cycleViewEnabled: command.CycleViewEnabled,
            issueViewsViewEnabled: command.IssueViewsViewEnabled,
            pageViewEnabled: command.PageViewEnabled,
            intakeViewEnabled: command.IntakeViewEnabled,
            guestViewAllFeatures: command.GuestViewAllFeatures,
            isTimeTrackingEnabled: command.IsTimeTrackingEnabled,
            isIssueTypeEnabled: command.IsIssueTypeEnabled,
            archiveIn: command.ArchiveIn,
            closeIn: command.CloseIn);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return ProjectDtoMapper.ToDto(project);
    }
}
