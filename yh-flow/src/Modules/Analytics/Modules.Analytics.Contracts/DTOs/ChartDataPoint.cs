using System.Text.Json.Serialization;

namespace YH.Modules.Analytics.Contracts.DTOs;

public sealed class ChartDataPoint
{
    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("count")] public int Count { get; set; }
    [JsonPropertyName("completed_issues")] public int CompletedIssues { get; set; }
    [JsonPropertyName("created_issues")] public int CreatedIssues { get; set; }
}
