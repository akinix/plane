using System.Text.Json.Serialization;

namespace YH.Modules.Page.Contracts.DTOs;

/// <summary>
/// Page detail DTO (extends PageDto with description fields, corresponds to Plane PageDetailSerializer).
/// </summary>
public class PageDetailDto : PageDto
{
    /// <summary>Full HTML description content.</summary>
    [JsonPropertyName("description_html")]
    public string? DescriptionHtml { get; set; }

    /// <summary>Plain-text version of the description.</summary>
    [JsonPropertyName("description_stripped")]
    public string? DescriptionStripped { get; set; }

    /// <summary>JSON description content (editor state sync).</summary>
    [JsonPropertyName("description_json")]
    public string? DescriptionJson { get; set; }
}