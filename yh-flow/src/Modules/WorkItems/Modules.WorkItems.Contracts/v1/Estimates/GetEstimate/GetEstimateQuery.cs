using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Estimates.GetEstimate;

/// <summary>
/// Get a single estimate by id with its estimate points (REQ-4.7).
/// <see cref="ProjectId"/> is populated from the route by the endpoint.
/// </summary>
public sealed class GetEstimateQuery : IQuery<EstimateDto>
{
    /// <summary>Route segment {projectId} — set by the endpoint, not client-writable.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment {estimateId}.</summary>
    [JsonIgnore]
    public Guid EstimateId { get; set; }
}
