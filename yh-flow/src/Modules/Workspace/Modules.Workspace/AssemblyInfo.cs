using YH.Framework.Web.Modules;

// Order=200 — between Multitenancy (~100) and Auditing (300). The slug strategy/store wiring in
// WorkspaceModule.ConfigureServices must run AFTER MultitenancyModule has registered the
// AddMultiTenant<AppTenantInfo>() builder chain (spike Q1: external TryAddEnumerable appends to it).
[assembly: FshModule(typeof(YH.Modules.Workspace.WorkspaceModule), 200)]
