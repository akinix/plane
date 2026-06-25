using YH.Framework.Web.Modules;

// Order=260 — between Project (250) and Auditing (300). The Page module depends on
// project-level and workspace-level infrastructure (ICurrentWorkspaceContext, [RequireWorkspaceRole])
// which are already wired by Phase 2/3.
[assembly: FshModule(typeof(YH.Modules.Page.PageModule), 260)]