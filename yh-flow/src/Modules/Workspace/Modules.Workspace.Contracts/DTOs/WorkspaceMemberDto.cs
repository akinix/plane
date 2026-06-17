using YH.Modules.Identity.Contracts.DTOs;

namespace YH.Modules.Workspace.Contracts.DTOs;

/// <summary>
/// Workspace membership response DTO (REQ-2.2).
/// Field set mirrors Plane <c>WorkspaceMember</c> serializer: id, role (numeric 20/15/5 per D-11),
/// is_active. <see cref="User"/> is joined server-side via <c>IUserIdentityService.GetUsersByIdsAsync</c>
/// (D-04/D-05) to avoid an N+1 when listing members (RESEARCH Pitfall 3). Null <see cref="User"/>
/// indicates the referenced user id could not be resolved in Identity (treat as "user deleted").
/// </summary>
public class WorkspaceMemberDto
{
    public Guid Id { get; set; }

    /// <summary>Membership role as integer (matches Plane <c>ROLE_CHOICES</c>: 20=Admin, 15=Member, 5=Guest).</summary>
    public int Role { get; set; }

    /// <summary>Whether the membership is active (soft-removed members are inactive).</summary>
    public bool IsActive { get; set; }

    /// <summary>Referenced user summary (joined from Identity via D-04/D-05 batch lookup).</summary>
    public UserSummary? User { get; set; }

    /// <summary>Workspace id this membership belongs to (tenant of the row).</summary>
    public Guid WorkspaceId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
