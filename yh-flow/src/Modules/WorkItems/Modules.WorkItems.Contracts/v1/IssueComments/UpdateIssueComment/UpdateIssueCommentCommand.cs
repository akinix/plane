using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.IssueComments.UpdateIssueComment;

/// <summary>
/// Update an IssueComment.
/// <see cref="IssueId"/>, <see cref="CommentId"/>, and <see cref="ProjectId"/> are populated
/// from the route by the endpoint. Only the comment author can edit.
/// </summary>
public sealed class UpdateIssueCommentCommand : ICommand<IssueCommentDto>
{
    /// <summary>Updated comment HTML body (required, sanitized by handler).</summary>
    public string CommentHtml { get; set; } = default!;

    /// <summary>Updated JSON format body (optional).</summary>
    public string? CommentJson { get; set; }

    /// <summary>Route segment {issueId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid IssueId { get; set; }

    /// <summary>Route segment {commentId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid CommentId { get; set; }

    /// <summary>Route segment {projectId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }
}
