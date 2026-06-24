using System.Text.Json.Serialization;

namespace YH.Modules.WorkItems.Contracts.DTOs;

/// <summary>
/// Estimate response DTO.
/// Field set mirrors Plane <c>serializers/estimate.py</c> EstimateSerializer.
/// JsonPropertyName attributes follow Plane JSON naming convention (snake_case).
/// </summary>
public class EstimateDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    /// <summary>Estimate type: "points" or "categories".</summary>
    public string Type { get; set; } = default!;

    /// <summary>Project that owns this estimate (scalar Guid, no cross-module FK).</summary>
    public Guid ProjectId { get; set; }

    /// <summary>Whether this estimate is the project's current active estimate.</summary>
    public bool IsLastUsed { get; set; }

    /// <summary>Collection of estimate points (null when listing estimates without points).</summary>
    public List<EstimatePointDto>? EstimatePoints { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
