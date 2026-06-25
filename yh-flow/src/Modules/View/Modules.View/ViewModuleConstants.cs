using YH.Framework.Web.Modules;

namespace YH.Modules.View;

/// <summary>
/// View module constants.
/// </summary>
public sealed class ViewModuleConstants : IModuleConstants
{
    public string ModuleId => "View";

    public string ModuleName => "View";

    /// <summary>Top-level URL prefix (Plane: /api/v1/workspaces/{slug}/projects/{projectId}/views/).</summary>
    public string ApiPrefix => "views";

    /// <summary>DB schema name (per-module schema with yhschema. prefix).</summary>
    public const string SchemaName = "yhschema.View";
}
