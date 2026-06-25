using System.Text.Json.Serialization;

namespace YH.Modules.Page.Contracts.DTOs;

/// <summary>
/// Page list-version DTO (no description_html, corresponds to Plane PageSerializer).
/// JsonPropertyName attributes follow Plane JSON naming convention (snake_case).
/// </summary>
public class PageDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    /// <summary>Visibility: 0=Public, 1=Private.</summary>
    public int Access { get; set; }

    public string? Color { get; set; }

    [JsonPropertyName("is_locked")]
    public bool IsLocked { get; set; }

    [JsonPropertyName("is_global")]
    public bool IsGlobal { get; set; }

    [JsonPropertyName("sort_order")]
    public double SortOrder { get; set; }

    /// <summary>Owner user id (scalar, no cross-module FK).</summary>
    [JsonPropertyName("owned_by")]
    public Guid OwnedBy { get; set; }

    /// <summary>Parent page id (optional, for hierarchy).</summary>
    public Guid? ParentId { get; set; }

    [JsonPropertyName("archived_at")]
    public DateTimeOffset? ArchivedAt { get; set; }

    [JsonPropertyName("view_props")]
    public string? ViewProps { get; set; }

    [JsonPropertyName("logo_props")]
    public string? LogoProps { get; set; }

    public string? ExternalSource { get; set; }

    public string? ExternalId { get; set; }

    /// <summary>Populated at query time — whether the current user has favorited this page.</summary>
    [JsonPropertyName("is_favorite")]
    public bool IsFavorite { get; set; }

    // Audit timestamps (Plane JSON naming convention)

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}