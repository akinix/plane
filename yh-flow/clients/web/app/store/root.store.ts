// FLOW: CoreRootStore — root MobX store (per CONVENTIONS.md)
import { AuthStore, type IAuthStore } from "./auth.store";
import { WorkspaceStore } from "./workspace.store";
import { ProjectStore } from "./project.store";
import type { IWorkspaceStore, IProjectStore } from "./types";

export interface ICoreRootStore {
  auth: IAuthStore;
  workspace: IWorkspaceStore;
  project: IProjectStore;
}

export class CoreRootStore implements ICoreRootStore {
  auth: IAuthStore;
  workspace: IWorkspaceStore;
  project: IProjectStore;

  constructor() {
    this.auth = new AuthStore();
    this.workspace = new WorkspaceStore();
    this.project = new ProjectStore();
  }
}
