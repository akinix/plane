using YH.Framework.Web.Modules;

// Order=310 — after View (290) and before Auditing (300). Analytics depends on
// WorkItemsDbContext for real-time aggregation queries and Workspace for
// [RequireWorkspaceRole] and ICurrentWorkspaceContext.
[assembly: FshModule(typeof(YH.Modules.Analytics.AnalyticsModule), 310)]
