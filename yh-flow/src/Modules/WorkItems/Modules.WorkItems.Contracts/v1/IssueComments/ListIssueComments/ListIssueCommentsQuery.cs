using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.IssueComments.ListIssueComments;

/// <summary>
/// List IssueComments for a specific issue.
/// Results are ordered by CreatedOnUtc ascending (oldest first).
/// </summary>
public sealed class ListIssueCommentsQuery : IQuery<List<IssueCommentDto>>
{
    /// <summary>Route segment {issueId} — set by the endpoint, not client-writable.</summary>
    [JsonIgnore]
    public Guid IssueId { get; set; }

    /// <summary>Route segment {projectId} — set by the endpoint, not client-writable.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }
}
