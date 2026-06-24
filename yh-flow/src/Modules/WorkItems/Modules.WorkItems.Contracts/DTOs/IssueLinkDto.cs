using System.Text.Json.Serialization;

namespace YH.Modules.WorkItems.Contracts.DTOs;

/// <summary>
/// IssueLink response DTO — combined external-link + internal-relation pattern.
/// Field set mirrors Plane issue_link + issue_relation models merged per CONTEXT decision.
/// </summary>
public class IssueLinkDto
{
    public Guid Id { get; set; }

    [JsonPropertyName("issue_id")]
    public Guid IssueId { get; set; }

    [JsonPropertyName("related_issue_id")]
    public Guid? RelatedIssueId { get; set; }

    public string? Url { get; set; }

    public string? Title { get; set; }

    [JsonPropertyName("link_type")]
    public int LinkType { get; set; }

    public string? Metadata { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}
