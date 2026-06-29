using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.Cycles.CreateCycle;

/// <summary>
/// Create a project cycle. <see cref="ProjectId"/> is populated from the route by the endpoint.
/// </summary>
public sealed class CreateCycleCommand : ICommand<CreateCycleResponse>
{
    /// <summary>Cycle name (max 255, non-empty).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Optional description (max 10000 chars).</summary>
    public string? Description { get; set; }

    /// <summary>Optional start date (must be paired with EndDate).</summary>
    public DateTimeOffset? StartDate { get; set; }

    /// <summary>Optional end date (must be paired with StartDate).</summary>
    public DateTimeOffset? EndDate { get; set; }

    /// <summary>Timezone for date calculations (default "UTC").</summary>
    public string? Timezone { get; set; }

    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint, not client-writable.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }
}

public sealed record CreateCycleResponse(Guid Id);
