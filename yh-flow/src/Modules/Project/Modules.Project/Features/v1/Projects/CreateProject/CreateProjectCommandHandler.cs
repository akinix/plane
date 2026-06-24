using System.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Project.Contracts;
using YH.Modules.Project.Contracts.v1.Projects.CreateProject;
using YH.Modules.Project.Data;
using YH.Modules.Project.Domain;
using YH.Modules.Workspace.Contracts;
using ProjectEntity = YH.Modules.Project.Domain.Project;

namespace YH.Modules.Project.Features.v1.Projects.CreateProject;

/// <summary>
/// Handles <see cref="CreateProjectCommand"/> — creates the project aggregate, auto-enrols the
/// creator as the first Admin <see cref="ProjectMember"/> (D-06), and persists both in one
/// SaveChanges unit of work.
/// </summary>
/// <remarks>
/// <para><b>Slug resolution:</b> if <see cref="CreateProjectCommand.Slug"/> is provided, it is used
/// directly; otherwise a slug is generated from <see cref="CreateProjectCommand.Name"/>.</para>
/// <para><b>Auto Admin member (D-06):</b> the creator is automatically added as a
/// <see cref="ProjectMember"/> with Admin role. If <see cref="CreateProjectCommand.ProjectLeadId"/>
/// differs from the owner, the lead is also added as Admin.</para>
/// </remarks>
public sealed class CreateProjectCommandHandler : ICommandHandler<CreateProjectCommand, CreateProjectResponse>
{
    private readonly ProjectDbContext _db;

    public CreateProjectCommandHandler(ProjectDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<CreateProjectResponse> Handle(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.OwnerUserId == Guid.Empty)
        {
            // Defensive: endpoint must populate OwnerUserId from the authenticated principal.
            throw new ArgumentException("Owner user id is required.", nameof(command));
        }

        // Resolve slug — explicit takes precedence; otherwise generate from name.
        var slug = string.IsNullOrWhiteSpace(command.Slug)
            ? Slugify(command.Name)
            : command.Slug;

        // Check identifier uniqueness within workspace (tenant-scoped by DbContext).
        if (await _db.Projects.AnyAsync(p => p.Identifier == command.Identifier, cancellationToken).ConfigureAwait(false))
        {
            throw new CustomException(
                "Project identifier is already taken.",
                Array.Empty<string>(),
                HttpStatusCode.Conflict);
        }

        var project = ProjectEntity.Create(
            name: command.Name,
            slug: slug,
            identifier: command.Identifier,
            ownerId: command.OwnerUserId,
            description: command.Description,
            descriptionText: command.DescriptionText,
            descriptionHtml: command.DescriptionHtml,
            network: (ProjectNetwork)command.Network,
            projectLeadId: command.ProjectLeadId,
            defaultAssigneeId: command.DefaultAssigneeId,
            emoji: command.Emoji,
            iconProp: command.IconProp,
            coverImageUrl: command.CoverImageUrl,
            logoProps: command.LogoProps,
            timeZone: command.TimeZone);

        _db.Projects.Add(project);

        // D-06 — auto-enrol the creator as Admin ProjectMember in the same SaveChanges unit.
        var ownerMember = ProjectMember.Create(
            projectId: project.Id,
            userId: command.OwnerUserId.ToString(),
            role: (int)WorkspaceRole.Admin,
            isActive: true);
        _db.Members.Add(ownerMember);

        // If project_lead is different from owner, add them as Admin too (Plane behaviour).
        if (command.ProjectLeadId.HasValue && command.ProjectLeadId.Value != command.OwnerUserId)
        {
            var leadMember = ProjectMember.Create(
                projectId: project.Id,
                userId: command.ProjectLeadId.Value.ToString(),
                role: (int)WorkspaceRole.Admin,
                isActive: true);
            _db.Members.Add(leadMember);
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CreateProjectResponse(project.Id, project.Slug);
    }

    /// <summary>
    /// Simple slug generation from a display name. Converts to lowercase, replaces non-alphanumeric
    /// chars with hyphens, collapses consecutive hyphens, and truncates to 100 chars.
    /// </summary>
    private static string Slugify(string value)
    {
        var trimmed = value.Trim();
#pragma warning disable CA1308 // Slug is canonical lowercase, not security-sensitive
        var lower = trimmed.ToLowerInvariant();
#pragma warning restore CA1308
        var chars = lower.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        var collapsed = new string(chars).Trim('-');
        while (collapsed.Contains("--", StringComparison.Ordinal))
        {
            collapsed = collapsed.Replace("--", "-", StringComparison.Ordinal);
        }

        return collapsed.Length > 100 ? collapsed[..100] : collapsed;
    }
}
