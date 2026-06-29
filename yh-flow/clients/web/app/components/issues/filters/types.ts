// FLOW: Filter engine type definitions — domain-agnostic filter config for all 5 views
// per D-P17-09 ~ D-P17-15

/**
 * Filter criteria for Issue queries
 */
export type TFilterCriteria = {
  stateIds: string[];
  priorityIds: string[];
  assigneeIds: string[];
  labelIds: string[];
  searchQuery: string;
  dateRange: { start?: string; end?: string } | null;
};

/** Sort configuration */
export type TSortConfig = {
  sortBy: string;
  sortDirection: "asc" | "desc";
};

/** Group-by options shared across all 5 views */
export type TGroupByOptions = "state" | "priority" | "assignees" | "created_by" | "none";

/** Sub-group (swimlane) options for Kanban view */
export type TSubGroupByOptions = "state" | "priority" | "none";

/** All 5 view layouts */
export type TViewLayout = "list" | "kanban" | "calendar" | "gantt" | "spreadsheet";

/** Persisted view configuration */
export type TIssueView = {
  id: string;
  name: string;
  projectId: string;
  filters: TFilterCriteria;
  sort: TSortConfig;
  groupBy: TGroupByOptions;
  subGroupBy: TSubGroupByOptions;
  displayColumns: string[];
  layout: TViewLayout;
  createdAt: string;
  updatedAt: string;
};

/** Column visibility configuration */
export type TColumnVisibility = {
  columns: string[];
};
