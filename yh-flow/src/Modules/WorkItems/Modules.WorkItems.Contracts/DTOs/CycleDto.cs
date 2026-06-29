using System.Text.Json.Serialization;

namespace YH.Modules.WorkItems.Contracts.DTOs;

/// <summary>
/// Cycle response DTO.
/// Field set mirrors Plane <c>serializers/cycle.py</c> CycleSerializer.
/// JsonPropertyName attributes follow Plane JSON naming convention (snake_case).
/// </summary>
public class CycleDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }

    public double SortOrder { get; set; } = 65535.0;

    public string? ExternalSource { get; set; }

    public string? ExternalId { get; set; }

    public string? ProgressSnapshot { get; set; }

    public DateTimeOffset? ArchivedAt { get; set; }

    public string? LogoProps { get; set; }

    public string Timezone { get; set; } = "UTC";

    public int Version { get; set; } = 1;

    // Computed / progress fields

    /// <summary>Computed cycle status (e.g. "in_progress", "completed").</summary>
    public string? Status { get; set; }

    /// <summary>Total issues in this cycle.</summary>
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

    /// <summary>Whether this cycle is favorited by the current user.</summary>
    public bool IsFavorite { get; set; }

    /// <summary>Assignee user IDs associated with this cycle.</summary>
    public List<string> AssigneeIds { get; set; } = [];

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
