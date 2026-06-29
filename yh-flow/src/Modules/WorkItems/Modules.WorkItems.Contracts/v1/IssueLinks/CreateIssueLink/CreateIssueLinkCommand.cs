using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.IssueLinks.CreateIssueLink;

/// <summary>
/// Create an issue link (external URL or internal relation, REQ-4.4).
/// <see cref="IssueId"/> is populated from the route by the endpoint.
/// At least one of <see cref="RelatedIssueId"/> or <see cref="Url"/> must be provided (handler validates).
/// <see cref="RelatedIssueId"/> is validated to exist in the same project.
/// Requires workspace Admin or Member role.
/// </summary>
public sealed class CreateIssueLinkCommand : ICommand<IssueLinkDto>
{
    /// <summary>Route segment {issueId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid IssueId { get; set; }

    /// <summary>Link type: RelatesTo=0, Duplicate=1, Blocks=2, BlockedBy=3.</summary>
    public int LinkType { get; set; }

    /// <summary>Related issue id (for internal Issue-to-Issue relations).</summary>
    public Guid? RelatedIssueId { get; set; }

    /// <summary>External URL (for external links, max 2048 chars).</summary>
    public string? Url { get; set; }

    /// <summary>Display title (optional, max 255 chars).</summary>
    public string? Title { get; set; }

    /// <summary>Optional JSON metadata (max 4000 chars).</summary>
    public string? Metadata { get; set; }

    /// <summary>Validated project id — set from the route context in the handler.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }
}
