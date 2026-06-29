using System.Text.Json.Serialization;

namespace YH.Modules.WorkItems.Contracts.DTOs;

/// <summary>
/// EstimatePoint response DTO — value object representing a single point in an estimate scale.
/// Field set mirrors Plane <c>serializers/estimate.py</c> EstimatePointSerializer.
/// JsonPropertyName attributes follow Plane JSON naming convention (snake_case).
/// </summary>
public class EstimatePointDto
{
    public Guid Id { get; set; }

    /// <summary>Parent estimate id.</summary>
    public Guid EstimateId { get; set; }

    /// <summary>Sort key for the point (0, 1, 2...).</summary>
    public int Key { get; set; }

    /// <summary>Display value (e.g. "1", "2", "3", "S", "M", "L").</summary>
    public string Value { get; set; } = default!;

    /// <summary>Sort order for point listing UI (default 65535.0 per Plane convention).</summary>
    public double SortOrder { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
