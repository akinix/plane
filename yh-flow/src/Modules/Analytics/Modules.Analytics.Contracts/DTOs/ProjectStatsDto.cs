using System.Text.Json.Serialization;

namespace YH.Modules.Analytics.Contracts.DTOs;

public sealed class ProjectStatsDto
{
    [JsonPropertyName("project_id")] public Guid ProjectId { get; set; }
    [JsonPropertyName("project__name")] public string ProjectName { get; set; } = string.Empty;
    [JsonPropertyName("cancelled_work_items")] public int CancelledWorkItems { get; set; }
    [JsonPropertyName("completed_work_items")] public int CompletedWorkItems { get; set; }
    [JsonPropertyName("backlog_work_items")] public int BacklogWorkItems { get; set; }
    [JsonPropertyName("un_started_work_items")] public int UnStartedWorkItems { get; set; }
    [JsonPropertyName("started_work_items")] public int StartedWorkItems { get; set; }
}
