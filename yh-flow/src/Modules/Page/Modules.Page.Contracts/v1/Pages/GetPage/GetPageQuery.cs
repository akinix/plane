using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.Page.Contracts.DTOs;

namespace YH.Modules.Page.Contracts.v1.Pages.GetPage;

/// <summary>
/// Fetch a single page by id (REQ-7.1). PageId is populated from the route by the endpoint.
/// </summary>
public sealed class GetPageQuery : IQuery<PageDetailDto>
{
    /// <summary>Route segment {pageId} — set by the endpoint, not client-writable.</summary>
    [JsonIgnore]
    public Guid PageId { get; set; }
}