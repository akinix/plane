using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.IssueLinks.DeleteIssueLink;

/// <summary>
/// Delete an issue link (hard-delete, REQ-4.4).
/// <see cref="IssueId"/> and <see cref="LinkId"/> are populated from the route by the endpoint.
/// Requires workspace Admin role.
/// </summary>
public sealed class DeleteIssueLinkCommand : ICommand
{
    /// <summary>Route segment {issueId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid IssueId { get; set; }

    /// <summary>Route segment {linkId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid LinkId { get; set; }
}
