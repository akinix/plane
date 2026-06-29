using YH.Modules.Project.Contracts.DTOs;
using ProjectEntity = YH.Modules.Project.Domain.Project;

namespace YH.Modules.Project.Features.v1.Projects;

/// <summary>
/// Maps a <see cref="ProjectEntity"/> domain entity to a <see cref="ProjectDto"/> contract DTO.
/// </summary>
/// <remarks>
/// Centralised so every feature slice (Get/List/Create/Update) produces an identical DTO shape.
/// Audit / soft-delete fields are projected to Plane-style property names
/// (<c>created_at</c> / <c>updated_at</c> / <c>deleted_at</c>) by the DTO's JSON serialization.
/// </remarks>
internal static class ProjectDtoMapper
{
    internal static ProjectDto ToDto(ProjectEntity p) =>
        new()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Network = (int)p.Network,
            Identifier = p.Identifier,
            Slug = p.Slug,
            OwnerId = p.OwnerId,
            ProjectLeadId = p.ProjectLeadId,
            DefaultAssigneeId = p.DefaultAssigneeId,
            Emoji = p.Emoji,
            IconProp = p.IconProp,
            CoverImageUrl = p.CoverImageUrl,
            LogoProps = p.LogoProps,
            TimeZone = p.TimeZone,
            ModuleViewEnabled = p.ModuleViewEnabled,
            CycleViewEnabled = p.CycleViewEnabled,
            IssueViewsViewEnabled = p.IssueViewsViewEnabled,
            PageViewEnabled = p.PageViewEnabled,
            IntakeViewEnabled = p.IntakeViewEnabled,
            GuestViewAllFeatures = p.GuestViewAllFeatures,
            IsTimeTrackingEnabled = p.IsTimeTrackingEnabled,
            IsIssueTypeEnabled = p.IsIssueTypeEnabled,
            ArchiveIn = p.ArchiveIn,
            CloseIn = p.CloseIn,
            ArchivedAt = p.ArchivedAt,
            SortOrder = p.SortOrder,
            CreatedAt = p.CreatedOnUtc,
            UpdatedAt = p.LastModifiedOnUtc,
            DeletedAt = p.DeletedOnUtc,
        };

    /// <summary>
    /// Maps to DTO with membership annotation for list queries.
    /// </summary>
    /// <param name="p">The project entity.</param>
    /// <param name="isMember">Whether the current user is a project member.</param>
    /// <param name="memberRole">The current user's role in the project, if any.</param>
    internal static ProjectDto ToDto(ProjectEntity p, bool isMember, int? memberRole)
    {
        var dto = ToDto(p);
        dto.IsMember = isMember;
        dto.MemberRole = memberRole;
        return dto;
    }
}
