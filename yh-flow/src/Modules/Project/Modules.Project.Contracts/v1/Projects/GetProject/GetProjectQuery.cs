using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.Project.Contracts.DTOs;

namespace YH.Modules.Project.Contracts.v1.Projects.GetProject;

/// <summary>
/// Fetch a single project by id (REQ-3.1). <see cref="ProjectId"/> is populated from the route by the endpoint.
/// </summary>
public sealed class GetProjectQuery : IQuery<ProjectDto>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint, not client-writable.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }
}
