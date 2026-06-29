using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.Page.Contracts.DTOs;

namespace YH.Modules.Page.Contracts.v1.Pages.ListPages;

/// <summary>
/// List pages in a project, with optional archived/parent filters.
/// </summary>
public sealed class ListPagesQuery : IQuery<List<PageDto>>
{
    /// <summary>Project id — set by the endpoint from the route.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Filter: show only archived pages when true (default: active pages only).</summary>
    public bool IsArchived { get; set; }

    /// <summary>Filter by parent page id. null = top-level pages only.</summary>
    public Guid? Parent { get; set; }

    /// <summary>Page number (Plane-style pagination).</summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>Page size (Plane-style pagination).</summary>
    public int PageSize { get; set; } = 30;
}