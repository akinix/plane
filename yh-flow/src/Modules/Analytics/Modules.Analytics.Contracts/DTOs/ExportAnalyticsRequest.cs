using System.Text.Json.Serialization;

namespace YH.Modules.Analytics.Contracts.DTOs;

public sealed class ExportAnalyticsRequest
{
    [JsonPropertyName("x_axis")] public string? XAxis { get; set; }
    [JsonPropertyName("y_axis")] public string? YAxis { get; set; }
    [JsonPropertyName("segment")] public string? Segment { get; set; }
}
