using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.Labels.CreateLabel;

/// <summary>
/// Create a project label (REQ-4.2). <see cref="ProjectId"/> is populated from the route by the endpoint.
/// Mutations require workspace Admin role per CONTEXT.
/// </summary>
public sealed class CreateLabelCommand : ICommand<CreateLabelResponse>
{
    /// <summary>Display name (max 255, non-empty). Unique per project.</summary>
    public string Name { get; set; } = default!;

    /// <summary>Optional hex color code (e.g. "#46A758"). Max 7 chars.</summary>
    public string? Color { get; set; }

    /// <summary>Parent label id for hierarchy support (self-referencing FK).</summary>
    public Guid? ParentId { get; set; }

    /// <summary>Optional description (max 1000 chars).</summary>
    public string? Description { get; set; }

    /// <summary>Sort order for label listing UI. Default 65535.0.</summary>
    public double? SortOrder { get; set; }

    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint, not client-writable.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }
}
