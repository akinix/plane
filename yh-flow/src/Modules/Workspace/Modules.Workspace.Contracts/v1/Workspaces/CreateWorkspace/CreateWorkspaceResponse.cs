namespace YH.Modules.Workspace.Contracts.v1.Workspaces.CreateWorkspace;

/// <summary>
/// Response for POST /api/v1/workspaces/ — the created workspace's id + final slug.
/// </summary>
/// <param name="Id">Created workspace Guid.</param>
/// <param name="Slug">Final slug (either client-provided, validated, or generator-derived).</param>
public sealed record CreateWorkspaceResponse(Guid Id, string Slug);
