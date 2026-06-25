using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.Page.Contracts.DTOs;

namespace YH.Modules.Page.Contracts.v1.Pages.GetPageDescription;

/// <summary>
/// Get a page's description content (html + stripped + json).
/// </summary>
public sealed class GetPageDescriptionQuery : IQuery<PageDetailDto>
{
    /// <summary>Route segment {pageId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid PageId { get; set; }
}