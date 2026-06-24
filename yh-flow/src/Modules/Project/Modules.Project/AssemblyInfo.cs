using YH.Framework.Web.Modules;

// Order=250 — between Workspace (200) and Auditing (300). The Project module depends on
// workspace-level infrastructure (ICurrentWorkspaceContext, [RequireWorkspaceRole]) and
// identity contracts (IUserIdentityService) which are already wired by Phase 1/2.
[assembly: FshModule(typeof(YH.Modules.Project.ProjectModule), 250)]
