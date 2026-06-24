using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.v1.EstimatePoints.CreateEstimatePoint;

namespace YH.Modules.WorkItems.Contracts.v1.Estimates.CreateEstimate;

/// <summary>
/// Create an estimate system (REQ-4.7).
/// <see cref="ProjectId"/> is populated from the route by the endpoint.
/// Optionally seeds initial estimate points via <see cref="InitialPoints"/>.
/// Requires workspace Admin or Member role.
/// </summary>
public sealed class CreateEstimateCommand : ICommand<CreateEstimateResponse>
{
    /// <summary>Display name (max 255, required).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Estimate type: "points" or "categories" (required).</summary>
    public string Type { get; set; } = default!;

    /// <summary>Optional initial estimate points to seed.</summary>
    public List<InitialEstimatePoint>? InitialPoints { get; set; }

    /// <summary>Route segment {projectId} — set by the endpoint, not client-writable.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }
}

/// <summary>
/// Describes an initial estimate point for seeding during estimate creation.
/// </summary>
public sealed class InitialEstimatePoint
{
    /// <summary>Sort key (0, 1, 2...).</summary>
    public int Key { get; set; }

    /// <summary>Display value (e.g. "1", "2", "3", "S", "M", "L").</summary>
    public string Value { get; set; } = default!;
}
