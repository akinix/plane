using System.Text.Json.Serialization;

namespace YH.Modules.WorkItems.Contracts.DTOs;

/// <summary>
/// Label response DTO.
/// Field set mirrors Plane <c>serializers/issue.py</c> LabelSerializer.
/// JsonPropertyName attributes follow Plane JSON naming convention (snake_case).
/// </summary>
public class LabelDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string? Color { get; set; }

    /// <summary>Parent label id for hierarchy support (self-referencing FK).</summary>
    public Guid? ParentId { get; set; }

    /// <summary>Project that owns this label (scalar Guid, no cross-module FK).</summary>
    public Guid ProjectId { get; set; }

    /// <summary>Sort order for label listing UI (default 65535.0 per Plane convention).</summary>
    public double SortOrder { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
