namespace YH.Modules.Workspace.Contracts.v1.Workspaces.CheckWorkspaceSlug;

/// <summary>
/// Response for slug-availability check.
/// </summary>
/// <param name="Exists">True when the slug is taken by an ACTIVE workspace; false when available.</param>
public sealed record CheckWorkspaceSlugResponse(bool Exists);
