// FLOW: Store type interfaces for WorkspaceStore, ProjectStore, and IssueStore (UI state only per D-P15-08)
import type { IProject, IWorkspace } from "@plane/types";
import type { IIssueStore } from "./issue.store";
import type { ICycleStore } from "./cycle.store";
import type { IModuleStore } from "./module.store";
import type { TViewLayout, TGroupByOptions, TFilterCriteria } from "@/components/issues/filters/types";

export type { IIssueStore, ICycleStore, IModuleStore, TViewLayout, TGroupByOptions, TFilterCriteria };

export interface IWorkspaceStore {
  workspaceSwitcherOpen: boolean;
  sidebarCollapsed: boolean;
  expandedWorkspaceIds: string[];
  currentWorkspaceId: string | null;
  workspaces: IWorkspace[];

  setSidebarCollapsed: (collapsed: boolean) => void;
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
