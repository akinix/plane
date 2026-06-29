using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Cycles.ListCycles;

/// <summary>
/// List cycles in a project, ordered by SortOrder ascending.
/// Supports optional cycle_view filter (current/upcoming/completed/draft/incomplete/all).
/// </summary>
public sealed class ListCyclesQuery : IQuery<List<CycleDto>>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Optional cycle view filter: "current", "upcoming", "completed", "draft", "incomplete", "all".</summary>
    public string? CycleView { get; set; }
}
