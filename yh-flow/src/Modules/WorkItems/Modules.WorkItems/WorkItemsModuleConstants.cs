using YH.Framework.Web.Modules;

namespace YH.Modules.WorkItems;

/// <summary>
/// WorkItems module constants.
/// </summary>
public sealed class WorkItemsModuleConstants : IModuleConstants
{
    public string ModuleId => "WorkItems";

    public string ModuleName => "WorkItems";

    /// <summary>Top-level URL prefix (Plane: <c>/api/v1/workspaces/{slug}/projects/{projectId}/work-items/</c>).</summary>
    public string ApiPrefix => "work-items";

    /// <summary>DB schema name (per-module schema with <c>yhschema.</c> prefix).</summary>
    public const string SchemaName = "yhschema.WorkItems";
}
