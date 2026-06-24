using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.IssueActivities.ListIssueActivities;

/// <summary>
/// List IssueActivities for a specific issue.
/// Results are ordered by Epoch descending (most recent first).
/// </summary>
public sealed class ListIssueActivitiesQuery : IQuery<List<IssueActivityDto>>
{
    /// <summary>Route segment {issueId} — set by the endpoint, not client-writable.</summary>
    [JsonIgnore]
    public Guid IssueId { get; set; }

    /// <summary>Route segment {projectId} — set by the endpoint, not client-writable.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }
}
