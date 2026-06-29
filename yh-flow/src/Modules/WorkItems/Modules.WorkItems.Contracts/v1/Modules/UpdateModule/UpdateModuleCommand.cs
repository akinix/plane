using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Modules.UpdateModule;

/// <summary>
/// Update mutable module fields (PATCH semantics). All fields are optional.
/// <see cref="ProjectId"/> and <see cref="ModuleId"/> are populated from the route.
/// </summary>
public sealed class UpdateModuleCommand : ICommand<ModuleDto>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment <c>{moduleId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ModuleId { get; set; }

    /// <summary>Display name (max 255).</summary>
    public string? Name { get; set; }

    /// <summary>Optional description (max 10000 chars).</summary>
    public string? Description { get; set; }

    /// <summary>Module status — "backlog", "planned", "in-progress", "paused", "completed", "cancelled".</summary>
    public string? Status { get; set; }

    /// <summary>Optional start date (no pairing rule with TargetDate).</summary>
    public DateTimeOffset? StartDate { get; set; }

    /// <summary>Optional target date (no pairing rule with StartDate).</summary>
    public DateTimeOffset? TargetDate { get; set; }

    /// <summary>Module lead/owner.</summary>
    public Guid? LeadId { get; set; }

    /// <summary>Sort order for module listing UI.</summary>
    public double? SortOrder { get; set; }
}
