using System.Text.Json.Serialization;

namespace YH.Modules.WorkItems.Contracts.DTOs;

/// <summary>
/// Module response DTO — mirrors Plane <c>serializers/module.py</c> ModuleSerializer.
/// JsonPropertyName attributes follow Plane JSON naming convention (snake_case).
/// </summary>
public class ModuleDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    /// <summary>Module status — default "planned".</summary>
    public string Status { get; set; } = "planned";

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? TargetDate { get; set; }

    public double SortOrder { get; set; } = 65535.0;

    /// <summary>Module lead/owner (scalar Guid, no cross-module FK).</summary>
    public Guid? LeadId { get; set; }

    /// <summary>Frozen JSON progress snapshot.</summary>
    public string? ProgressSnapshot { get; set; }

    /// <summary>When this module was archived (null if active).</summary>
    public DateTimeOffset? ArchivedAt { get; set; }

    /// <summary>Logo/image properties (JSON).</summary>
    public string? LogoProps { get; set; }

    /// <summary>Optimistic concurrency version (default 1).</summary>
    public int Version { get; set; } = 1;

    /// <summary>Project that owns this module.</summary>
    public Guid ProjectId { get; set; }

    // Computed / progress fields (calculated at query time)

    /// <summary>Total issues in this module.</summary>
    public int TotalIssues { get; set; }

    /// <summary>Number of completed issues.</summary>
    public int CompletedIssues { get; set; }

    /// <summary>Number of cancelled issues.</summary>
    public int CancelledIssues { get; set; }

    /// <summary>Number of started issues.</summary>
    public int StartedIssues { get; set; }

    /// <summary>Number of unstarted issues.</summary>
    public int UnstartedIssues { get; set; }

    /// <summary>Number of backlog issues.</summary>
    public int BacklogIssues { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
