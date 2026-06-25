using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.Page.Contracts.v1.Pages.ArchivePage;

/// <summary>
/// Archive a page. PageId is populated from the route by the endpoint.
/// </summary>
public sealed class ArchivePageCommand : ICommand<bool>
{
    /// <summary>Route segment {pageId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid PageId { get; set; }
}