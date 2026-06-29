using System.Text.Json.Serialization;

namespace YH.Modules.WorkItems.Contracts.DTOs;

/// <summary>
/// IssueComment response DTO.
/// </summary>
public class IssueCommentDto
{
    public Guid Id { get; set; }

    [JsonPropertyName("issue_id")]
    public Guid IssueId { get; set; }

    [JsonPropertyName("comment_html")]
    public string CommentHtml { get; set; } = default!;

    [JsonPropertyName("comment_json")]
    public string? CommentJson { get; set; }

    [JsonPropertyName("comment_stripped")]
    public string? CommentStripped { get; set; }

    [JsonPropertyName("actor_id")]
    public string ActorId { get; set; } = default!;

    [JsonPropertyName("parent_id")]
    public Guid? ParentId { get; set; }

    [JsonPropertyName("edited_at")]
    public DateTimeOffset? EditedAt { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonPropertyName("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }
}
