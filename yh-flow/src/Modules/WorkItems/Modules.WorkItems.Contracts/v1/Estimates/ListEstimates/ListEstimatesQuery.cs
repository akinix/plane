using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Estimates.ListEstimates;

/// <summary>
/// List all estimate systems for a project (REQ-4.7).
/// <see cref="ProjectId"/> is populated from the route by the endpoint.
/// </summary>
public sealed class ListEstimatesQuery : IQuery<List<EstimateDto>>
{
    /// <summary>Route segment {projectId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }
}
