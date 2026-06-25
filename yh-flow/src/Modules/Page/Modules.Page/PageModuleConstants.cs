using YH.Framework.Web.Modules;

namespace YH.Modules.Page;

/// <summary>
/// Page module constants.
/// </summary>
public sealed class PageModuleConstants : IModuleConstants
{
    public string ModuleId => "Page";

    public string ModuleName => "Page";

    /// <summary>Top-level URL prefix (Plane: /api/v1/workspaces/{slug}/projects/{projectId}/pages/).</summary>
    public string ApiPrefix => "pages";

    /// <summary>DB schema name (per-module schema with yhschema. prefix).</summary>
    public const string SchemaName = "yhschema.Page";
}