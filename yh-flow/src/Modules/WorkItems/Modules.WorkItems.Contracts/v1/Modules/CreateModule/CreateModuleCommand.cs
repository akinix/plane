using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.Modules.CreateModule;

/// <summary>
/// Create a project module. <see cref="ProjectId"/> is populated from the route by the endpoint.
/// </summary>
public sealed class CreateModuleCommand : ICommand<CreateModuleResponse>
{
    /// <summary>Module name (max 255, non-empty).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Optional description (max 10000 chars).</summary>
    public string? Description { get; set; }

    /// <summary>Module status — default "planned" by the handler if null.</summary>
    public string? Status { get; set; }

    /// <summary>Optional start date (no pairing rule with TargetDate).</summary>
    public DateTimeOffset? StartDate { get; set; }

    /// <summary>Optional target date (no pairing rule with StartDate).</summary>
    public DateTimeOffset? TargetDate { get; set; }

    /// <summary>Module lead/owner (scalar Guid, no cross-module FK).</summary>
    public Guid? LeadId { get; set; }

    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint, not client-writable.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }
}
