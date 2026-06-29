using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.EstimatePoints.DeleteEstimatePoint;

/// <summary>
/// Delete an estimate point (REQ-4.7).
/// <see cref="EstimatePointId"/> is populated from the route by the endpoint.
/// Returns 409 Conflict if any Issue references this EstimatePoint.
/// </summary>
public sealed class DeleteEstimatePointCommand : ICommand
{
    /// <summary>Route segment {estimatePointId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid EstimatePointId { get; set; }
}
