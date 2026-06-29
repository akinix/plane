using System.Text.Json.Serialization;

namespace YH.Modules.WorkItems.Contracts.DTOs;

/// <summary>
/// Issue-Label mapping DTO.
/// </summary>
public class IssueLabelDto
{
    public Guid Id { get; set; }

    [JsonPropertyName("issue_id")]
    public Guid IssueId { get; set; }

    [JsonPropertyName("label_id")]
    public Guid LabelId { get; set; }

    [JsonPropertyName("label_name")]
    public string? LabelName { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}
