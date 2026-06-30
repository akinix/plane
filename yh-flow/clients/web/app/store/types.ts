// FLOW: Store type interfaces for WorkspaceStore, ProjectStore, and IssueStore (UI state only per D-P15-08)
import type { IProject, IWorkspace } from "@plane/types";
import type { IIssueStore } from "./issue.store";
import type { ICycleStore } from "./cycle.store";
import type { IModuleStore } from "./module.store";
import type { IPageStore } from "./page.store";
import type { IViewStore } from "./view.store";
import type { INotificationStore } from "./notification.store";
import type { IAnalyticsStore } from "./analytics.store";
import type { TViewLayout, TGroupByOptions, TFilterCriteria } from "@/components/issues/filters/types";

export type {
  IIssueStore,
  ICycleStore,
  IModuleStore,
  IPageStore,
  IViewStore,
  INotificationStore,
  IAnalyticsStore,
  TViewLayout,
  TGroupByOptions,
  TFilterCriteria,
};

export interface IWorkspaceStore {
  workspaceSwitcherOpen: boolean;
  sidebarCollapsed: boolean;
  expandedWorkspaceIds: string[];
  currentWorkspaceId: string | null;
  workspaces: IWorkspace[];

  setSidebarCollapsed: (collapsed: boolean) => void;
  toggleSidebarCollapsed: () => void;
  setCurrentWorkspace: (id: string) => void;
  setWorkspaceSwitcherOpen: (open: boolean) => void;
  toggleWorkspaceExpand: (id: string) => void;
}

export interface IProjectStore {
  expandedProjectIds: string[];
  projectFilterText: string;
  projects: IProject[];

  setExpandedProject: (id: string) => void;
  setProjectFilterText: (text: string) => void;
}
