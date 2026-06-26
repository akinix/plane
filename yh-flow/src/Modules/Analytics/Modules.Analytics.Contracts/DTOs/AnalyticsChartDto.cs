using System.Text.Json.Serialization;

namespace YH.Modules.Analytics.Contracts.DTOs;

public sealed class AnalyticsChartDto
{
    [JsonPropertyName("data")] public List<ChartDataPoint> Data { get; set; } = [];
    [JsonPropertyName("schema")] public Dictionary<string, string> Schema { get; set; } = new();
}
