using System.Text.Json.Serialization;

namespace YH.Modules.View.Contracts.DTOs;

/// <summary>
/// View list-version DTO (no filters/display_filters, corresponds to Plane IssueViewSerializer).
/// JsonPropertyName attributes follow Plane JSON naming convention (snake_case).
/// </summary>
public class ViewDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    /// <summary>Visibility: 0=Private, 1=Public.</summary>
    public int Access { get; set; }

    [JsonPropertyName("sort_order")]
    public double SortOrder { get; set; }

    [JsonPropertyName("logo_props")]
    public string? LogoProps { get; set; }

    /// <summary>Owner user id (scalar, no cross-module FK).</summary>
    [JsonPropertyName("owned_by")]
    public Guid OwnedBy { get; set; }

    [JsonPropertyName("is_locked")]
    public bool IsLocked { get; set; }

    [JsonPropertyName("archived_at")]
    public DateTimeOffset? ArchivedAt { get; set; }

    [JsonPropertyName("project_id")]
    public Guid? ProjectId { get; set; }

    /// <summary>Populated at query time — whether the current user has favorited this view.</summary>
    [JsonPropertyName("is_favorite")]
    public bool IsFavorite { get; set; }

    // Audit timestamps (Plane JSON naming convention)

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}