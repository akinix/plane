using System.Text.Json.Serialization;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.DTOs;

/// <summary>
/// IntakeIssue response DTO.
/// </summary>
public class IntakeIssueDto
{
    public Guid Id { get; set; }

    [JsonPropertyName("issue_id")]
    public Guid IssueId { get; set; }

    [JsonPropertyName("project_id")]
    public Guid ProjectId { get; set; }

    /// <summary>IntakeIssueStatus as int: -2 Pending, -1 Rejected, 0 Snoozed, 1 Accepted, 2 Duplicate.</summary>
    public int Status { get; set; }

    [JsonPropertyName("snoozed_till")]
    public DateTime? SnoozedTill { get; set; }

    [JsonPropertyName("duplicate_to_issue_id")]
    public Guid? DuplicateToIssueId { get; set; }

    public string Source { get; set; } = "IN_APP";

    /// <summary>Nested IssueDto (populated on list/detail).</summary>
    public IssueDto? Issue { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
