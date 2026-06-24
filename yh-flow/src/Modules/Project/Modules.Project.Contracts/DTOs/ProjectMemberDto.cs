using System.Text.Json.Serialization;
using YH.Modules.Identity.Contracts.DTOs;

namespace YH.Modules.Project.Contracts.DTOs;

/// <summary>
/// Project member response DTO.
/// Field set mirrors Plane <c>api/serializers/member.py</c> ProjectMemberSerializer.
/// </summary>
public class ProjectMemberDto
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public Guid UserId { get; set; }

    /// <summary>Membership role as integer (matches WorkspaceRole: 20=Admin, 15=Member, 5=Guest).</summary>
    public int Role { get; set; }

    /// <summary>Whether the membership is active (soft-removed members are inactive).</summary>
    public bool IsActive { get; set; }

    /// <summary>Referenced user summary (joined from Identity via batch lookup per D-04/D-05).</summary>
    public UserSummary? User { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
