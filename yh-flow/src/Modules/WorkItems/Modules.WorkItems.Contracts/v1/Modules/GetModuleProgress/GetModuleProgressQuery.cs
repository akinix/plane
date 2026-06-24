using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Modules.GetModuleProgress;

/// <summary>
/// Query the real-time progress of a module. <see cref="ProjectId"/> and <see cref="ModuleId"/> are populated from the route.
/// Returns aggregated issue counts grouped by StateGroup, plus completion percentage.
/// </summary>
public sealed class GetModuleProgressQuery : IQuery<ModuleProgressDto>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment <c>{moduleId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ModuleId { get; set; }
}
