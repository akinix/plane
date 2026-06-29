using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Modules.GetModule;

/// <summary>
/// Fetch a single module by id. <see cref="ProjectId"/> and <see cref="ModuleId"/> are populated from the route.
/// </summary>
public sealed class GetModuleQuery : IQuery<ModuleDto>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment <c>{moduleId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ModuleId { get; set; }
}
