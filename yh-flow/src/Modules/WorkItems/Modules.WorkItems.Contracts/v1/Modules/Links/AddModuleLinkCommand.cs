using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.Modules.Links;

/// <summary>
/// Add a link to a module. <see cref="ProjectId"/> and <see cref="ModuleId"/> are populated from the route.
/// </summary>
public sealed class AddModuleLinkCommand : ICommand<Unit>
{
    /// <summary>Link title (required, max 255).</summary>
    public string Title { get; set; } = default!;

    /// <summary>Link URL (required, max 2048).</summary>
    public string Url { get; set; } = default!;

    /// <summary>Optional metadata (JSON string).</summary>
    public string? Metadata { get; set; }

    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment <c>{moduleId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ModuleId { get; set; }
}
