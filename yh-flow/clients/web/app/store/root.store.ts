// FLOW: CoreRootStore — root MobX store (per CONVENTIONS.md)
import { AuthStore, type IAuthStore } from "./auth.store";
import { WorkspaceStore } from "./workspace.store";
import { ProjectStore } from "./project.store";
import { IssueStore } from "./issue.store";
import { CycleStore } from "./cycle.store";
import { ModuleStore } from "./module.store";
import { PageStore } from "./page.store";
import { ViewStore } from "./view.store";
import { NotificationStore, type INotificationStore } from "./notification.store";
import type {
  IWorkspaceStore,
  IProjectStore,
  IIssueStore,
  ICycleStore,
  IModuleStore,
  IPageStore,
  IViewStore,
} from "./types";

export interface ICoreRootStore {
  auth: IAuthStore;
  workspace: IWorkspaceStore;
  project: IProjectStore;
  issue: IIssueStore;
  cycle: ICycleStore;
  module: IModuleStore;
  page: IPageStore;
  view: IViewStore;
  notification: INotificationStore;
}

export class CoreRootStore implements ICoreRootStore {
  auth: IAuthStore;
  workspace: IWorkspaceStore;
  project: IProjectStore;
  issue: IIssueStore;
  cycle: ICycleStore;
  module: IModuleStore;
  page: IPageStore;
  view: IViewStore;
  notification: INotificationStore;

  constructor() {
    this.auth = new AuthStore();
    this.workspace = new WorkspaceStore();
    this.project = new ProjectStore();
    this.issue = new IssueStore();
    this.cycle = new CycleStore();
    this.module = new ModuleStore();
    this.page = new PageStore();
    this.view = new ViewStore();
    this.notification = new NotificationStore();
  }
}
