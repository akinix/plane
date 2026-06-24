using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.IssueLinks.ListIssueLinks;

/// <summary>
/// List all links for an issue (REQ-4.4).
/// <see cref="IssueId"/> is populated from the route by the endpoint.
/// </summary>
public sealed class ListIssueLinksQuery : IQuery<List<IssueLinkDto>>
{
    /// <summary>Route segment {issueId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid IssueId { get; set; }
}
