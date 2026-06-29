// FLOW: CoreRootStore — root MobX store (per CONVENTIONS.md)
import { AuthStore, type IAuthStore } from "./auth.store";
import { WorkspaceStore } from "./workspace.store";
import { ProjectStore } from "./project.store";
import { IssueStore } from "./issue.store";
import type { IWorkspaceStore, IProjectStore, IIssueStore } from "./types";

export interface ICoreRootStore {
  auth: IAuthStore;
  workspace: IWorkspaceStore;
  project: IProjectStore;
  issue: IIssueStore;
}

export class CoreRootStore implements ICoreRootStore {
  auth: IAuthStore;
  workspace: IWorkspaceStore;
  project: IProjectStore;
  issue: IIssueStore;

  constructor() {
    this.auth = new AuthStore();
    this.workspace = new WorkspaceStore();
    this.project = new ProjectStore();
    this.issue = new IssueStore();
  }
}
