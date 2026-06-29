using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.EstimatePoints.CreateEstimatePoint;

/// <summary>
/// Create an estimate point within an estimate system (REQ-4.7).
/// <see cref="EstimateId"/> is populated from the route by the endpoint.
/// </summary>
public sealed class CreateEstimatePointCommand : ICommand<EstimatePointDto>
{
    /// <summary>Route segment {estimateId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid EstimateId { get; set; }

    /// <summary>Sort key (0, 1, 2...). Must be unique within the estimate.</summary>
    public int Key { get; set; }

    /// <summary>Display value (e.g. "1", "2", "S", "M", "L"). Max 50 chars, required.</summary>
    public string Value { get; set; } = default!;
}
