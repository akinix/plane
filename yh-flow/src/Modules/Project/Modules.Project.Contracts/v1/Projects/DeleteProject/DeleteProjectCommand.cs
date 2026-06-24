using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.Project.Contracts.v1.Projects.DeleteProject;

/// <summary>
/// Soft-delete a project (REQ-3.1, D-03). Only project Admins or workspace Admins may delete.
/// </summary>
/// <remarks>
/// <see cref="ProjectId"/> and <see cref="CurrentUserId"/> are populated by the endpoint from the route
/// and the authenticated principal respectively.
/// </remarks>
public sealed class DeleteProjectCommand : ICommand
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Authenticated user id — set by the endpoint for the owner/Admin check.</summary>
    [JsonIgnore]
    public Guid CurrentUserId { get; set; }
}
