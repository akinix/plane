using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.IssueComments.DeleteIssueComment;

/// <summary>
/// Delete (soft-delete) an IssueComment.
/// <see cref="IssueId"/>, <see cref="CommentId"/>, and <see cref="ProjectId"/> are populated
/// from the route by the endpoint. Only the comment author or an Admin can delete.
/// </summary>
public sealed class DeleteIssueCommentCommand : ICommand
{
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
