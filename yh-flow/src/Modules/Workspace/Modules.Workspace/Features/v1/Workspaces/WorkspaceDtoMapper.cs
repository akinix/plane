using YH.Modules.Workspace.Contracts.DTOs;
using WorkspaceEntity = YH.Modules.Workspace.Domain.Workspace;

namespace YH.Modules.Workspace.Features.v1.Workspaces;

/// <summary>
/// Maps a <see cref="WorkspaceEntity"/> domain entity to a <see cref="WorkspaceDto"/> contract DTO.
/// </summary>
/// <remarks>
/// Centralised so every feature slice (Get/List/Create/Update) produces an identical DTO shape.
/// Audit / soft-delete fields are projected to Plane-style property names
/// (<c>created_at</c> / <c>updated_at</c> / <c>deleted_at</c>) by the DTO's JSON serialization.
/// </remarks>
internal static class WorkspaceDtoMapper
{
    internal static WorkspaceDto ToDto(WorkspaceEntity ws) =>
        new()
        {
            Id = ws.Id,
            Name = ws.Name,
            Slug = ws.Slug,
            OwnerId = ws.OwnerId,
            Logo = ws.Logo,
            OrganizationSize = ws.OrganizationSize,
            TimeZone = ws.TimeZone,
            BackgroundColor = ws.BackgroundColor,
            CreatedAt = ws.CreatedOnUtc,
            UpdatedAt = ws.LastModifiedOnUtc,
            DeletedAt = ws.DeletedOnUtc,
        };
}
