// FLOW: Store type interfaces for WorkspaceStore and ProjectStore (UI state only per D-P15-08)
import type { IProject, IWorkspace } from "@plane/types";

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
