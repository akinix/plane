using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Cycles.GetCycleProgress;

/// <summary>
/// Query the progress of a cycle. <see cref="CycleId"/> is populated from the route.
/// </summary>
public sealed class GetCycleProgressQuery : IQuery<CycleProgressDto>
{
    /// <summary>Route segment <c>{cycleId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid CycleId { get; set; }

    /// <summary>Chart type (default "issues").</summary>
    public string? Type { get; set; }
}
