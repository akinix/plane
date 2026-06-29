using YH.Framework.Web.Modules;

// Order=290 — between Page (260) and Auditing (300). The View module depends on
// project-level and workspace-level infrastructure (ICurrentWorkspaceContext, [RequireWorkspaceRole])
// which are already wired by Phase 2/3.
[assembly: FshModule(typeof(YH.Modules.View.ViewModule), 290)]
