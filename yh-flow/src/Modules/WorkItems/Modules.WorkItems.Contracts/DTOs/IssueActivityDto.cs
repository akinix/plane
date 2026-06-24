using System.Text.Json.Serialization;

namespace YH.Modules.WorkItems.Contracts.DTOs;

/// <summary>
/// IssueActivity response DTO.
/// </summary>
public class IssueActivityDto
{
    public Guid Id { get; set; }

    [JsonPropertyName("issue_id")]
    public Guid IssueId { get; set; }

    public string Verb { get; set; } = default!;

    public string? Field { get; set; }

    [JsonPropertyName("old_value")]
    public string? OldValue { get; set; }

    [JsonPropertyName("new_value")]
    public string? NewValue { get; set; }

    public string? Comment { get; set; }

    [JsonPropertyName("actor_id")]
    public string ActorId { get; set; } = default!;

    [JsonPropertyName("issue_comment_id")]
    public Guid? IssueCommentId { get; set; }

    public long Epoch { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}
