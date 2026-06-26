using System.Text.Json.Serialization;

namespace YH.Modules.Analytics.Contracts.DTOs;

public sealed class AssigneeStatsDto
{
    [JsonPropertyName("display_name")] public string? DisplayName { get; set; }
    [JsonPropertyName("assignee_id")] public Guid? AssigneeId { get; set; }
    [JsonPropertyName("avatar_url")] public string? AvatarUrl { get; set; }
    [JsonPropertyName("cancelled_work_items")] public int CancelledWorkItems { get; set; }
    [JsonPropertyName("completed_work_items")] public int CompletedWorkItems { get; set; }
    [JsonPropertyName("backlog_work_items")] public int BacklogWorkItems { get; set; }
    [JsonPropertyName("un_started_work_items")] public int UnStartedWorkItems { get; set; }
    [JsonPropertyName("started_work_items")] public int StartedWorkItems { get; set; }
}
