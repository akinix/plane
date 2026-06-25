using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.Page.Contracts.v1.Pages.GetPageSummary;

/// <summary>
/// Get a summary (counts + recently updated) for pages in a project.
/// </summary>
public sealed class GetPageSummaryQuery : IQuery<object>
{
    /// <summary>Project id — set by the endpoint from the route.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }
}