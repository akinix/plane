using System.Text.Json.Serialization;

namespace YH.Modules.WorkItems.Contracts.DTOs;

/// <summary>
/// Issue-Assignee mapping DTO.
/// </summary>
public class IssueAssigneeDto
{
    public Guid Id { get; set; }

    [JsonPropertyName("issue_id")]
    public Guid IssueId { get; set; }

    [JsonPropertyName("assignee_id")]
    public string AssigneeId { get; set; } = default!;

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}
