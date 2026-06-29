using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Issues.GetIssue;

/// <summary>
/// Get a single issue by id with detail fields (REQ-4.3).
/// <see cref="ProjectId"/> and <see cref="IssueId"/> are populated from the route by the endpoint.
/// Returns <see cref="IssueDetailDto"/> with assignees, labels, state_name, sequence_id_display.
/// </summary>
public sealed class GetIssueQuery : IQuery<IssueDetailDto>
{
    /// <summary>Route segment {projectId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment {issueId}.</summary>
    [JsonIgnore]
    public Guid IssueId { get; set; }
}
