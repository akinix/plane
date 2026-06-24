using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.Labels.DeleteLabel;

/// <summary>
/// Soft-delete a project label (REQ-4.2). <see cref="ProjectId"/> and <see cref="LabelId"/> are populated from the route.
/// Requires workspace Admin role per CONTEXT.
/// </summary>
public sealed class DeleteLabelCommand : ICommand
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment <c>{labelId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid LabelId { get; set; }
}
