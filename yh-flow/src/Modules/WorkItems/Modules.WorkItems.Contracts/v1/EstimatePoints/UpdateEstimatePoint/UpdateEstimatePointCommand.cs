using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.EstimatePoints.UpdateEstimatePoint;

/// <summary>
/// Update an estimate point (REQ-4.7).
/// <see cref="EstimatePointId"/> is populated from the route by the endpoint.
/// </summary>
public sealed class UpdateEstimatePointCommand : ICommand<EstimatePointDto>
{
    /// <summary>Route segment {estimatePointId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid EstimatePointId { get; set; }

    /// <summary>Updated sort key (optional).</summary>
    public int? Key { get; set; }

    /// <summary>Updated display value (optional).</summary>
    public string? Value { get; set; }
}
