using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.Modules.Issues;

/// <summary>
/// Remove an issue from a module. <see cref="ProjectId"/>, <see cref="ModuleId"/> and <see cref="IssueId"/> are populated from the route.
/// </summary>
public sealed class RemoveIssueFromModuleCommand : ICommand<Unit>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment <c>{moduleId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ModuleId { get; set; }

    /// <summary>Route segment <c>{issueId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid IssueId { get; set; }
}
