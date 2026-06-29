using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.Page.Contracts.DTOs;

namespace YH.Modules.Page.Contracts.v1.Pages.UpdatePage;

/// <summary>
/// Update a page's mutable fields (PATCH semantics). PageId is populated from the route by the endpoint.
/// </summary>
public sealed class UpdatePageCommand : ICommand<PageDetailDto>
{
    /// <summary>Route segment {pageId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid PageId { get; set; }

    /// <summary>Display name (optional, only applied if non-null).</summary>
    public string? Name { get; set; }

    /// <summary>Optional HTML description content.</summary>
    public string? DescriptionHtml { get; set; }

    /// <summary>Optional plain-text description.</summary>
    public string? DescriptionStripped { get; set; }

    /// <summary>Optional JSON description content.</summary>
    public string? DescriptionJson { get; set; }

    /// <summary>Visibility: 0=Public, 1=Private.</summary>
    public int? Access { get; set; }

    /// <summary>Optional hex color.</summary>
    public string? Color { get; set; }

    /// <summary>Optional parent page id.</summary>
    public Guid? ParentId { get; set; }

    /// <summary>Sort order for listing.</summary>
    public double? SortOrder { get; set; }

    /// <summary>Optional JSON view configuration.</summary>
    public string? ViewProps { get; set; }

    /// <summary>Optional JSON logo configuration.</summary>
    public string? LogoProps { get; set; }

    /// <summary>Whether this page is workspace-wide visible.</summary>
    public bool? IsGlobal { get; set; }
}