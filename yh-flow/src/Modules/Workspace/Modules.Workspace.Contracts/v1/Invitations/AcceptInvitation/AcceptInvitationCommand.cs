using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.Workspace.Contracts.v1.Invitations.AcceptInvitation;

/// <summary>
/// Accept a workspace invitation (REQ-2.4, threat T-2-acceptpublic + T-2-acceptdouble).
/// </summary>
/// <remarks>
/// <see cref="Token"/> is the RAW invitation token from the URL path segment (the raw token is
/// NEVER re-readable from the database — only its SHA-256 hash is). The handler hashes the
/// inbound token, looks the invitation up by hash, and rejects if the invitation is no longer
/// <c>IsValid</c> (accepted / revoked / rejected / expired). On success it transitions the
/// invitation to Accepted + creates the new <c>WorkspaceMember</c> row in the same unit of work.
/// </remarks>
public sealed class AcceptInvitationCommand : ICommand<AcceptInvitationResponse>
{
    /// <summary>Raw invitation token (from URL path).</summary>
    public string Token { get; set; } = default!;

    /// <summary>Authenticated user id — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid CurrentUserId { get; set; }
}

/// <summary>Result of <see cref="AcceptInvitationCommand"/> — the new member + workspace ids.</summary>
public sealed record AcceptInvitationResponse(Guid MemberId, Guid WorkspaceId);
