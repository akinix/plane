// FLOW: ProjectStore — MobX UI state for project tree/filter (per D-P15-08)
import { action, makeObservable, observable } from "mobx";
import type { IProject } from "@plane/types";
import type { IProjectStore } from "./types";

export class ProjectStore implements IProjectStore {
  expandedProjectIds: string[] = [];
  projectFilterText: string = "";
  projects: IProject[] = [];

  constructor() {
    makeObservable(this, {
      expandedProjectIds: observable,
      projectFilterText: observable.ref,
      projects: observable,
      setExpandedProject: action,
      setProjectFilterText: action,
    });
  }

  setExpandedProject = (id: string): void => {
    const index = this.expandedProjectIds.indexOf(id);
    if (index >= 0) {
      this.expandedProjectIds.splice(index, 1);
    } else {
      this.expandedProjectIds.push(id);
    }
  };

  setProjectFilterText = (text: string): void => {
    this.projectFilterText = text;
  };
}
