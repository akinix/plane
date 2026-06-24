using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.Modules.Issues;

/// <summary>
/// Add issues to a module. <see cref="ProjectId"/> and <see cref="ModuleId"/> are populated from the route.
/// Module has no COMPLETED restriction (unlike Cycle) — issues can be added at any time.
/// </summary>
public sealed class AddIssuesToModuleCommand : ICommand<Unit>
{
    /// <summary>Issue ids to add to the module.</summary>
    public List<Guid> IssueIds { get; set; } = [];

    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment <c>{moduleId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ModuleId { get; set; }
}
