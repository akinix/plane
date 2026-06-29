using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.Modules.Links;

/// <summary>
/// Remove a link from a module. <see cref="ProjectId"/>, <see cref="ModuleId"/> and <see cref="LinkId"/> are populated from the route.
/// </summary>
public sealed class RemoveModuleLinkCommand : ICommand<Unit>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment <c>{moduleId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ModuleId { get; set; }

    /// <summary>Route segment <c>{linkId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid LinkId { get; set; }
}
