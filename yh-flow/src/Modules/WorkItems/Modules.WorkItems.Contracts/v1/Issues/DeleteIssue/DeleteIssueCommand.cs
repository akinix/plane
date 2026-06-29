using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.Issues.DeleteIssue;

/// <summary>
/// Soft-delete an issue (REQ-4.3).
/// <see cref="ProjectId"/> and <see cref="IssueId"/> are populated from the route by the endpoint.
/// Cascade-deletes IssueAssignee, IssueLabel records via EF configuration.
/// Requires workspace Admin or Member role.
/// </summary>
public sealed class DeleteIssueCommand : ICommand
{
    /// <summary>Route segment {projectId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment {issueId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid IssueId { get; set; }
}
