// FLOW: WorkspaceStore — MobX UI state for workspace sidebar/navigation (per D-P15-08)
import { action, makeObservable, observable } from "mobx";
import type { IWorkspace } from "@plane/types";
import type { IWorkspaceStore } from "./types";

export class WorkspaceStore implements IWorkspaceStore {
  workspaceSwitcherOpen: boolean = false;
  sidebarCollapsed: boolean = false;
  expandedWorkspaceIds: string[] = [];
  currentWorkspaceId: string | null = null;
  workspaces: IWorkspace[] = [];

  constructor() {
    makeObservable(this, {
      workspaceSwitcherOpen: observable.ref,
      sidebarCollapsed: observable.ref,
      expandedWorkspaceIds: observable,
      currentWorkspaceId: observable.ref,
      workspaces: observable,
      setSidebarCollapsed: action,
      setCurrentWorkspace: action,
      setWorkspaceSwitcherOpen: action,
      toggleWorkspaceExpand: action,
    });
  }

  setSidebarCollapsed = (collapsed: boolean): void => {
    this.sidebarCollapsed = collapsed;
  };

  setCurrentWorkspace = (id: string): void => {
    this.currentWorkspaceId = id;
  };

  setWorkspaceSwitcherOpen = (open: boolean): void => {
    this.workspaceSwitcherOpen = open;
  };

  toggleWorkspaceExpand = (id: string): void => {
    const index = this.expandedWorkspaceIds.indexOf(id);
    if (index >= 0) {
      this.expandedWorkspaceIds.splice(index, 1);
    } else {
      this.expandedWorkspaceIds.push(id);
    }
  };
}
