using System.Text.Json.Serialization;

namespace YH.Modules.Analytics.Contracts.DTOs;

public sealed class WorkItemStatsDto
{
    [JsonPropertyName("total_work_items")] public CountValue TotalWorkItems { get; set; } = new();
    [JsonPropertyName("started_work_items")] public CountValue StartedWorkItems { get; set; } = new();
    [JsonPropertyName("backlog_work_items")] public CountValue BacklogWorkItems { get; set; } = new();
    [JsonPropertyName("un_started_work_items")] public CountValue UnStartedWorkItems { get; set; } = new();
    [JsonPropertyName("completed_work_items")] public CountValue CompletedWorkItems { get; set; } = new();
    [JsonPropertyName("cancelled_work_items")] public CountValue CancelledWorkItems { get; set; } = new();
}
