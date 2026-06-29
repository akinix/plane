namespace YH.Modules.Project.Contracts.v1.Projects.CreateProject;

/// <summary>
/// Response for POST /api/v1/workspaces/{slug}/projects/ — the created project's id and final slug.
/// </summary>
/// <param name="Id">Created project Guid.</param>
/// <param name="Slug">Final slug (either client-provided or auto-generated from name).</param>
public sealed record CreateProjectResponse(Guid Id, string Slug);
