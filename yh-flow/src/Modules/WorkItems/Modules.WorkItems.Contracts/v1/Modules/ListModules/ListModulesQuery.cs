using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Modules.ListModules;

/// <summary>
/// List modules in a project, ordered by SortOrder ascending.
/// Supports optional status filter (backlog/planned/in-progress/paused/completed/cancelled/all).
/// </summary>
public sealed class ListModulesQuery : IQuery<List<ModuleDto>>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Optional status filter: "backlog", "planned", "in-progress", "paused", "completed", "cancelled", "all".</summary>
    public string? Status { get; set; }
}
