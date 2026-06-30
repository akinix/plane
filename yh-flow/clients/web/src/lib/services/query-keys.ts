// FLOW: Centralized TanStack Query key factory (INFRA-03)
//
// Every module's query keys are defined here as const readonly tuples,
// ensuring type-safe, consistent, and IDE-refactorable cache key management.
// All keys are fully compatible with TanStack Query v5's QueryKey type
// (ReadonlyArray<unknown>).
//
// Usage:
//   import { queryKeys } from "@/lib/services/query-keys";
//   queryClient.invalidateQueries({ queryKey: queryKeys.issue.all() });

// ---------------------------------------------------------------------------
// Workspace
// ---------------------------------------------------------------------------
export const workspaceKeys = {
  /** Root key for invalidating all workspace queries */
  all: () => ["workspaces"] as const,
  /** Single workspace detail */
  detail: (id: string) => ["workspaces", id] as const,
  /** Current workspace */
  current: () => ["workspaces", "current"] as const,
};

// ---------------------------------------------------------------------------
// Project
// ---------------------------------------------------------------------------
export const projectKeys = {
  /** Root key for invalidating all project queries */
  all: () => ["projects"] as const,
  /** List of projects scoped to a workspace */
  list: (workspaceId: string) => ["projects", workspaceId] as const,
  /** Single project detail */
  detail: (workspaceId: string, projectId: string) => ["projects", workspaceId, projectId] as const,
};

// ---------------------------------------------------------------------------
// Member
// ---------------------------------------------------------------------------
export const memberKeys = {
  /** Root key for invalidating all member queries */
  all: () => ["members"] as const,
  /** List of members scoped to a workspace */
  list: (workspaceId: string) => ["members", workspaceId] as const,
};

// ---------------------------------------------------------------------------
// Issue
// ---------------------------------------------------------------------------
export const issueKeys = {
  /** Root key for invalidating all issue queries */
  all: () => ["issues"] as const,
  /** List of issues scoped to a project with optional filters */
  list: (projectId: string, filters?: Record<string, unknown>) => ["issues", projectId, filters] as const,
  /** Single issue detail */
  detail: (projectId: string, issueId: string) => ["issues", projectId, issueId] as const,
};

// ---------------------------------------------------------------------------
// State
// ---------------------------------------------------------------------------
export const stateKeys = {
  /** Root key for invalidating all state queries */
  all: () => ["states"] as const,
  /** List of states scoped to a project */
  list: (projectId: string) => ["states", projectId] as const,
};

// ---------------------------------------------------------------------------
// Label
// ---------------------------------------------------------------------------
export const labelKeys = {
  /** Root key for invalidating all label queries */
  all: () => ["labels"] as const,
  /** List of labels scoped to a project */
  list: (projectId: string) => ["labels", projectId] as const,
};

// ---------------------------------------------------------------------------
// Comment
// ---------------------------------------------------------------------------
export const commentKeys = {
  /** Root key for invalidating all comment queries */
  all: () => ["comments"] as const,
  /** List of comments scoped to an issue */
  list: (issueId: string) => ["comments", issueId] as const,
};

// ---------------------------------------------------------------------------
// Cycle
// ---------------------------------------------------------------------------
export const cycleKeys = {
  /** Root key for invalidating all cycle queries */
  all: () => ["cycles"] as const,
  /** List of cycles scoped to a project */
  list: (projectId: string) => ["cycles", projectId] as const,
  /** Single cycle detail */
  detail: (projectId: string, cycleId: string) => ["cycles", projectId, cycleId] as const,
  /** Cycle progress snapshot */
  progress: (projectId: string, cycleId: string) => ["cycles", projectId, cycleId, "progress"] as const,
};

// ---------------------------------------------------------------------------
// Module
// ---------------------------------------------------------------------------
export const moduleKeys = {
  /** Root key for invalidating all module queries */
  all: () => ["modules"] as const,
  /** List of modules scoped to a project */
  list: (projectId: string) => ["modules", projectId] as const,
  /** Single module detail */
  detail: (projectId: string, moduleId: string) => ["modules", projectId, moduleId] as const,
  /** Links associated with a module */
  links: (moduleId: string) => ["modules", moduleId, "links"] as const,
};

// ---------------------------------------------------------------------------
// Page
// ---------------------------------------------------------------------------
export const pageKeys = {
  /** Root key for invalidating all page queries */
  all: () => ["pages"] as const,
  /** List of pages scoped to a workspace */
  list: (workspaceId: string) => ["pages", workspaceId] as const,
  /** Single page detail */
  detail: (workspaceId: string, pageId: string) => ["pages", workspaceId, pageId] as const,
  /** Archived pages scoped to a workspace */
  archived: (workspaceId: string) => ["pages", "archived", workspaceId] as const,
};

// ---------------------------------------------------------------------------
// View
// ---------------------------------------------------------------------------
export const viewKeys = {
  /** Root key for invalidating all view queries */
  all: () => ["views"] as const,
  /** List of views scoped to a project */
  list: (projectId: string) => ["views", projectId] as const,
  /** Single view detail */
  detail: (viewId: string) => ["views", viewId] as const,
};

// ---------------------------------------------------------------------------
// Notification
// ---------------------------------------------------------------------------
export const notificationKeys = {
  /** Root key for invalidating all notification queries */
  all: () => ["notifications"] as const,
  /** List of notifications scoped to a workspace */
  list: (workspaceId: string) => ["notifications", "workspace", workspaceId] as const,
  /** Unread notification count scoped to a workspace */
  unreadCount: (workspaceId: string) => ["notifications", "unread-count", workspaceId] as const,
};

// ---------------------------------------------------------------------------
// Analytics
// ---------------------------------------------------------------------------
export const analyticsKeys = {
  /** Root key for invalidating all analytics queries */
  all: () => ["analytics"] as const,
  /** Dashboard analytics scoped to a workspace */
  dashboard: (workspaceId: string) => ["analytics", workspaceId] as const,
};

// ---------------------------------------------------------------------------
// Combined namespace
// ---------------------------------------------------------------------------
export const queryKeys = {
  workspace: workspaceKeys,
  project: projectKeys,
  member: memberKeys,
  issue: issueKeys,
  state: stateKeys,
  label: labelKeys,
  comment: commentKeys,
  cycle: cycleKeys,
  module: moduleKeys,
  page: pageKeys,
  view: viewKeys,
  notification: notificationKeys,
  analytics: analyticsKeys,
} as const;
