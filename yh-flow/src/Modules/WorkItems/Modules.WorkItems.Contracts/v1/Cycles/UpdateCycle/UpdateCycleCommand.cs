using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Cycles.UpdateCycle;

/// <summary>
/// Update mutable cycle fields (PATCH semantics). All fields are optional.
/// <see cref="ProjectId"/> and <see cref="CycleId"/> are populated from the route.
/// </summary>
public sealed class UpdateCycleCommand : ICommand<CycleDto>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment <c>{cycleId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid CycleId { get; set; }

    /// <summary>Display name (max 255).</summary>
    public string? Name { get; set; }

    /// <summary>Optional description (max 10000 chars).</summary>
    public string? Description { get; set; }

    /// <summary>Optional start date (must be paired with EndDate).</summary>
    public DateTimeOffset? StartDate { get; set; }

    /// <summary>Optional end date (must be paired with StartDate).</summary>
    public DateTimeOffset? EndDate { get; set; }

    /// <summary>Timezone for date calculations.</summary>
    public string? Timezone { get; set; }

    /// <summary>Sort order for cycle listing UI.</summary>
    public double? SortOrder { get; set; }
}
