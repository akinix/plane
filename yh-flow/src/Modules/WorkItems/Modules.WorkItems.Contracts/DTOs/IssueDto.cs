using System.Text.Json.Serialization;

namespace YH.Modules.WorkItems.Contracts.DTOs;

/// <summary>
/// Issue response DTO — flat projection with Plane-compatible snake_case JSON naming.
/// Field set mirrors Plane <c>serializers/issue.py</c> IssueSerializer.
/// </summary>
public class IssueDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    [JsonPropertyName("description_html")]
    public string? DescriptionHtml { get; set; }

    [JsonPropertyName("description_json")]
    public string? DescriptionJson { get; set; }

    [JsonPropertyName("description_stripped")]
    public string? DescriptionStripped { get; set; }

    /// <summary>Priority: "urgent" / "high" / "medium" / "low" / "none".</summary>
    public string Priority { get; set; } = "none";

    [JsonPropertyName("sequence_id")]
    public int SequenceId { get; set; }

    [JsonPropertyName("sort_order")]
    public double SortOrder { get; set; }

    [JsonPropertyName("project_id")]
    public Guid ProjectId { get; set; }

    [JsonPropertyName("parent_id")]
    public Guid? ParentId { get; set; }

    [JsonPropertyName("state_id")]
    public Guid? StateId { get; set; }

    [JsonPropertyName("estimate_point_id")]
    public Guid? EstimatePointId { get; set; }

    [JsonPropertyName("start_date")]
    public DateOnly? StartDate { get; set; }

    [JsonPropertyName("target_date")]
    public DateOnly? TargetDate { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTimeOffset? CompletedAt { get; set; }

    [JsonPropertyName("archived_at")]
    public DateTimeOffset? ArchivedAt { get; set; }

    [JsonPropertyName("is_draft")]
    public bool IsDraft { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonPropertyName("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }
}
