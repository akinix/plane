using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.IssueComments.CreateIssueComment;

/// <summary>
/// Create an IssueComment.
/// <see cref="IssueId"/> and <see cref="ProjectId"/> are populated from the route by the endpoint.
/// ActorId is populated from the authenticated user.
/// Requires workspace Admin, Member, or Guest role.
/// </summary>
public sealed class CreateIssueCommentCommand : ICommand<IssueCommentDto>
{
    /// <summary>Comment HTML body (required, sanitized by handler).</summary>
    public string CommentHtml { get; set; } = default!;

    /// <summary>JSON format body (optional).</summary>
    public string? CommentJson { get; set; }

    /// <summary>Parent comment id for nested replies (optional).</summary>
    public Guid? ParentId { get; set; }

    /// <summary>Route segment {issueId} — set by the endpoint, not client-writable.</summary>
    [JsonIgnore]
    public Guid IssueId { get; set; }

    /// <summary>Route segment {projectId} — set by the endpoint, not client-writable.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Authenticated user id — set by the endpoint, not client-writable.</summary>
    [JsonIgnore]
    public string ActorId { get; set; } = default!;
}
