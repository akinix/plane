using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.Page.Contracts.DTOs;

namespace YH.Modules.Page.Contracts.v1.Pages.UpdatePageDescription;

/// <summary>
/// Update a page's description content only.
/// </summary>
public sealed class UpdatePageDescriptionCommand : ICommand<PageDetailDto>
{
    /// <summary>Route segment {pageId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid PageId { get; set; }

    /// <summary>Optional HTML description content.</summary>
    public string? DescriptionHtml { get; set; }

    /// <summary>Optional plain-text description.</summary>
    public string? DescriptionStripped { get; set; }

    /// <summary>Optional JSON description content (editor state sync).</summary>
    public string? DescriptionJson { get; set; }
}