using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.Workspace.Contracts.v1.Workspaces.CreateWorkspace;

/// <summary>
/// Create a workspace (REQ-2.1 / D-06 / D-07 / D-09).
/// </summary>
/// <remarks>
/// <see cref="Slug"/> is optional — when omitted the handler generates a unique slug from
/// <see cref="Name"/> via <c>ISlugGenerator</c>. When provided, the generator still validates
/// it (format + restricted-words + uniqueness) and rejects invalid input.
/// <para><see cref="OwnerUserId"/> is populated by the endpoint from the authenticated user
/// (<c>[JsonIgnore]</c> so a caller cannot create a workspace on someone else's behalf).</para>
/// </remarks>
public sealed class CreateWorkspaceCommand : ICommand<CreateWorkspaceResponse>
{
    /// <summary>Display name (Plane: max 80, non-empty).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Optional explicit slug; if null/whitespace, derived from <see cref="Name"/>.</summary>
    public string? Slug { get; set; }

    /// <summary>Optional logo URL / asset reference.</summary>
    public string? Logo { get; set; }

    /// <summary>Optional IANA timezone (defaults to "UTC" when null).</summary>
    public string? TimeZone { get; set; }

    /// <summary>Authenticated creator user id — set by the endpoint, NOT client-writable.</summary>
    [JsonIgnore]
    public Guid OwnerUserId { get; set; }
}
