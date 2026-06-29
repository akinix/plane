using Mediator;

namespace YH.Modules.Workspace.Contracts.v1.Workspaces.CheckWorkspaceSlug;

/// <summary>
/// Check whether a slug is available for a new workspace (REQ-2.1). Mirrors Plane
/// <c>POST /api/v1/workspaces/slug-check/</c>.
/// </summary>
/// <remarks>
/// A slug "exists" if an ACTIVE (non-soft-deleted) workspace already owns it. Soft-deleted
/// workspaces released their slug via <c>__{epoch}</c> suffix (D-08), so the original slug returns
/// <see cref="CheckWorkspaceSlugResponse.Exists"/> = false (available).
/// </remarks>
public sealed class CheckWorkspaceSlugQuery : IQuery<CheckWorkspaceSlugResponse>
{
    /// <summary>Candidate slug to check.</summary>
    public string Slug { get; set; } = default!;
}
