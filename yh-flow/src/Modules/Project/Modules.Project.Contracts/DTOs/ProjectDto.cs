using System.Text.Json.Serialization;

namespace YH.Modules.Project.Contracts.DTOs;

/// <summary>
/// Project response DTO.
/// Field set mirrors Plane <c>api/serializers/project.py</c> ProjectSerializer.
/// JsonPropertyName attributes follow Plane JSON naming convention (snake_case).
/// </summary>
public class ProjectDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    /// <summary>Project visibility: 0=Secret, 2=Public.</summary>
    public int Network { get; set; }

    /// <summary>Short uppercase identifier prefix (e.g. "PROJ").</summary>
    public string Identifier { get; set; } = default!;

    /// <summary>URL-safe slug derived from name.</summary>
    public string Slug { get; set; } = default!;

    /// <summary>Owner user id (scalar, no cross-module FK per D-06).</summary>
    public Guid OwnerId { get; set; }

    /// <summary>Project lead user id (optional).</summary>
    public Guid? ProjectLeadId { get; set; }

    /// <summary>Default assignee user id (optional).</summary>
    public Guid? DefaultAssigneeId { get; set; }

    /// <summary>Emoji icon representation.</summary>
    public string? Emoji { get; set; }

    /// <summary>JSON icon configuration.</summary>
    public string? IconProp { get; set; }

    /// <summary>Cover image URL (optional).</summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>JSON logo configuration.</summary>
    public string? LogoProps { get; set; }

    /// <summary>IANA timezone identifier (default "UTC").</summary>
    public string TimeZone { get; set; } = "UTC";

    // Feature toggles

    public bool ModuleViewEnabled { get; set; }

    public bool CycleViewEnabled { get; set; }

    public bool IssueViewsViewEnabled { get; set; }

    public bool PageViewEnabled { get; set; } = true;

    public bool IntakeViewEnabled { get; set; }

    public bool GuestViewAllFeatures { get; set; }

    public bool IsTimeTrackingEnabled { get; set; }

    public bool IsIssueTypeEnabled { get; set; }

    // Archive / close settings

    public int ArchiveIn { get; set; }

    public int CloseIn { get; set; }

    public DateTimeOffset? ArchivedAt { get; set; }

    // List-annotation fields

    public bool IsMember { get; set; }

    public int? MemberRole { get; set; }

    public double? SortOrder { get; set; }

    // Audit timestamps (Plane JSON naming convention)

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonPropertyName("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }
}
