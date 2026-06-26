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

    // ── Priority distribution ─────────────────────────────────────────
    [JsonPropertyName("urgent_work_items")] public CountValue UrgentWorkItems { get; set; } = new();
    [JsonPropertyName("high_priority_work_items")] public CountValue HighPriorityWorkItems { get; set; } = new();
    [JsonPropertyName("medium_priority_work_items")] public CountValue MediumPriorityWorkItems { get; set; } = new();
    [JsonPropertyName("low_priority_work_items")] public CountValue LowPriorityWorkItems { get; set; } = new();
    [JsonPropertyName("none_priority_work_items")] public CountValue NonePriorityWorkItems { get; set; } = new();
}
