using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.Project.Contracts.v1.Members.RemoveMember;

/// <summary>
/// Remove (deactivate) a member from a project (REQ-3.2). Admin-driven; keeps the row for audit
/// by setting <c>IsActive = false</c> instead of hard-deleting.
/// </summary>
/// <remarks>
/// <para><see cref="ProjectId"/> and <see cref="MemberId"/> are populated by the endpoint
/// from route parameters (<c>[JsonIgnore]</c> so the client cannot override them).</para>
/// <para><see cref="CurrentUserId"/> is set by the endpoint from the authenticated principal
/// for the last-Admin guard.</para>
/// </remarks>
public sealed class RemoveMemberCommand : ICommand
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment <c>{memberId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid MemberId { get; set; }

    /// <summary>Authenticated user id — set by the endpoint; used for the last-Admin guard.</summary>
    [JsonIgnore]
    public Guid CurrentUserId { get; set; }
}
