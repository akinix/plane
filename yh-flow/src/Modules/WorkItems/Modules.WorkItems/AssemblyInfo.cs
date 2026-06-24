using YH.Framework.Web.Modules;

// Order=260 — between Project (250) and Auditing (300). The WorkItems module depends on
// workspace-level infrastructure (ICurrentWorkspaceContext, [RequireWorkspaceRole]),
// identity contracts (IUserIdentityService), and project contracts, all already wired by Phase 1-3.
[assembly: FshModule(typeof(YH.Modules.WorkItems.WorkItemsModule), 260)]
