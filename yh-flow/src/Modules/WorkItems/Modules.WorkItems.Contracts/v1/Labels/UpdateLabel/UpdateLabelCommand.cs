using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Labels.UpdateLabel;

/// <summary>
/// Update mutable label fields (REQ-4.2). All fields are optional (PATCH semantics).
/// <see cref="ProjectId"/> and <see cref="LabelId"/> are populated from the route.
/// Mutations require workspace Admin role per CONTEXT.
/// </summary>
public sealed class UpdateLabelCommand : ICommand<LabelDto>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment <c>{labelId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid LabelId { get; set; }

    /// <summary>Display name (max 255).</summary>
    public string? Name { get; set; }

    /// <summary>Optional hex color code (e.g. "#46A758").</summary>
    public string? Color { get; set; }

    /// <summary>Parent label id for hierarchy support.</summary>
    public Guid? ParentId { get; set; }

    /// <summary>Optional description (max 1000 chars).</summary>
    public string? Description { get; set; }

    /// <summary>Sort order for label listing UI.</summary>
    public double? SortOrder { get; set; }
}
