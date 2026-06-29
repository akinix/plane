using YH.Framework.Web.Modules;

namespace YH.Modules.Project;

/// <summary>
/// Project module constants.
/// </summary>
public sealed class ProjectModuleConstants : IModuleConstants
{
    public string ModuleId => "Project";

    public string ModuleName => "Project";

    /// <summary>Top-level URL prefix (Plane: <c>/api/v1/workspaces/{{slug}}/projects/</c>).</summary>
    public string ApiPrefix => "projects";

    /// <summary>DB schema name (Phase 1 schema strategy: per-module schema with <c>yhschema.</c> prefix).</summary>
    public const string SchemaName = "yhschema.Project";
}
