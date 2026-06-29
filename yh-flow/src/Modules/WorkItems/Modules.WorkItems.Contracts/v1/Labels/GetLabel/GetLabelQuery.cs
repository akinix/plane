using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Labels.GetLabel;

/// <summary>
/// Fetch a single label by id (REQ-4.2). <see cref="ProjectId"/> and <see cref="LabelId"/> are populated from the route.
/// </summary>
public sealed class GetLabelQuery : IQuery<LabelDto>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment <c>{labelId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid LabelId { get; set; }
}
