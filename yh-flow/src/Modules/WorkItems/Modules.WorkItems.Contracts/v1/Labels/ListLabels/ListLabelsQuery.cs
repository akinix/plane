using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Labels.ListLabels;

/// <summary>
/// List labels in a project (REQ-4.2), ordered by SortOrder ascending.
/// Supports optional ParentId filter for hierarchical queries.
/// </summary>
public sealed class ListLabelsQuery : IQuery<List<LabelDto>>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Optional filter by ParentId. When null, returns all labels flat (Plane behavior).
    /// When Guid.Empty, returns root labels only (no parent).
    /// </summary>
    public Guid? ParentId { get; set; }
}
