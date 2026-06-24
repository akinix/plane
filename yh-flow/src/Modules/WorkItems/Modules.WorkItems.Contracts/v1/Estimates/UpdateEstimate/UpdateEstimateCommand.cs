using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Estimates.UpdateEstimate;

/// <summary>
/// Update an estimate system (REQ-4.7).
/// <see cref="ProjectId"/> and <see cref="EstimateId"/> are populated from the route by the endpoint.
/// Mutable fields: Name, Type, IsLastUsed.
/// Requires workspace Admin role per CONTEXT (estimate mutations limited to Admin).
/// </summary>
public sealed class UpdateEstimateCommand : ICommand<EstimateDto>
{
    /// <summary>Route segment {projectId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment {estimateId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid EstimateId { get; set; }

    /// <summary>Updated display name (optional).</summary>
    public string? Name { get; set; }

    /// <summary>Updated estimate type: "points" or "categories" (optional).</summary>
    public string? Type { get; set; }

    /// <summary>Whether this estimate is the project's current active estimate (optional).</summary>
    public bool? IsLastUsed { get; set; }
}
