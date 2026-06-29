using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.Estimates.DeleteEstimate;

/// <summary>
/// Delete an estimate system (REQ-4.7).
/// <see cref="ProjectId"/> and <see cref="EstimateId"/> are populated from the route by the endpoint.
/// WARNING: Cascade-deletes all EstimatePoints.
/// Requires workspace Admin role per CONTEXT.
/// </summary>
public sealed class DeleteEstimateCommand : ICommand
{
    /// <summary>Route segment {projectId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment {estimateId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid EstimateId { get; set; }
}
