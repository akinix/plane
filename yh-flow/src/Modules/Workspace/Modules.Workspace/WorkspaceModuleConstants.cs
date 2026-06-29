using YH.Framework.Web.Modules;

namespace YH.Modules.Workspace;

/// <summary>
/// Workspace module constants (CONTEXT.md D-13 / D-09).
/// </summary>
public sealed class WorkspaceModuleConstants : IModuleConstants
{
    public string ModuleId => "Workspace";

    public string ModuleName => "Workspace";

    /// <summary>Top-level URL prefix (Plane: <c>/api/v1/workspaces/</c>).</summary>
    public string ApiPrefix => "workspaces";

    /// <summary>
    /// DB schema name (CONTEXT D-13 — Phase 1 schema strategy: per-module schema with <c>yhschema.</c> prefix).
    /// Used by every Workspace <c>IEntityTypeConfiguration</c>'s <c>ToTable(name, SchemaName)</c>.
    /// </summary>
    public const string SchemaName = "yhschema.Workspace";

    /// <summary>
    /// Maximum slug length (CONTEXT D-09). Matches Plane <c>Workspace.slug</c> field max length.
    /// Enforced in <c>WorkspaceConfiguration</c> via <c>HasMaxLength(SlugMaxLength)</c>.
    /// </summary>
    public const int SlugMaxLength = 48;
}
