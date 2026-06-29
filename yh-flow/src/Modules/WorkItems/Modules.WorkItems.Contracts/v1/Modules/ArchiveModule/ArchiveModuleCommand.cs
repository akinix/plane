using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.Modules.ArchiveModule;

/// <summary>
/// Archive a module. <see cref="ProjectId"/> and <see cref="ModuleId"/> are populated from the route.
/// Module has no date restrictions — any status can be archived (unlike Cycle).
/// </summary>
public sealed class ArchiveModuleCommand : ICommand<Unit>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment <c>{moduleId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ModuleId { get; set; }
}
