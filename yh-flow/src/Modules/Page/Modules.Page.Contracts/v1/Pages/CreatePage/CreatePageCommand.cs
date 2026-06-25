using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.Page.Contracts.v1.Pages.CreatePage;

/// <summary>
/// Create a new page (REQ-7.1). OwnedBy is populated by the endpoint from the authenticated user.
/// </summary>
public sealed class CreatePageCommand : ICommand<CreatePageResponse>
{
    /// <summary>Display name (Plane: max 255, non-empty).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Project id — set by the endpoint from the route {projectId}, not client-writable.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Authenticated creator user id — set by the endpoint, NOT client-writable.</summary>
    [JsonIgnore]
    public Guid OwnedBy { get; set; }

    /// <summary>Optional HTML description content.</summary>
    public string? DescriptionHtml { get; set; }

    /// <summary>Optional plain-text description.</summary>
    public string? DescriptionStripped { get; set; }

    /// <summary>Optional JSON description content (editor state sync).</summary>
    public string? DescriptionJson { get; set; }

    /// <summary>Visibility: 0=Public, 1=Private (default).</summary>
    public int Access { get; set; }

    /// <summary>Optional hex color.</summary>
    public string? Color { get; set; }

    /// <summary>Optional parent page id (for hierarchy).</summary>
    public Guid? ParentId { get; set; }

    /// <summary>Sort order for listing (Plane convention: 65535.0 default).</summary>
    public double SortOrder { get; set; } = 65535.0;

    /// <summary>Optional JSON view configuration.</summary>
    public string? ViewProps { get; set; }

    /// <summary>Optional JSON logo configuration.</summary>
    public string? LogoProps { get; set; }

    /// <summary>Whether this page is workspace-wide visible.</summary>
    public bool IsGlobal { get; set; }
}