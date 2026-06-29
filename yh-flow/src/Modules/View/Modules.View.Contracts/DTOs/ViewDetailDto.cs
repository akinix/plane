using System.Text.Json.Serialization;

namespace YH.Modules.View.Contracts.DTOs;

/// <summary>
/// View detail DTO (extends ViewDto with filters/display_filters, corresponds to Plane IssueViewSerializer).
/// </summary>
public class ViewDetailDto : ViewDto
{
    [JsonPropertyName("query")]
    public string? Query { get; set; }

    [JsonPropertyName("filters")]
    public string? Filters { get; set; }

    [JsonPropertyName("display_filters")]
    public string? DisplayFilters { get; set; }

    [JsonPropertyName("display_properties")]
    public string? DisplayProperties { get; set; }

    [JsonPropertyName("rich_filters")]
    public string? RichFilters { get; set; }
}