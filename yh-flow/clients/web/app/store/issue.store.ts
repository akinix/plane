// FLOW: IssueStore — MobX UI state for Issue views (per D-P16-02)
import { action, makeObservable, observable } from "mobx";

export interface IIssueStore {
  selectedIssueIds: string[];
  activeView: "list" | "kanban";
  currentPage: number;
  pageSize: number;
  groupBy: "state" | "priority" | "assignees";
  expandedColumnIds: string[];
  filters: {
    stateIds: string[];
    priorityIds: string[];
    assigneeIds: string[];
    searchQuery: string;
  };
  sortBy: string;
  sortDirection: "asc" | "desc";

  toggleIssueSelection: (id: string) => void;
  selectAll: (ids: string[]) => void;
  clearSelection: () => void;
  setActiveView: (view: "list" | "kanban") => void;
  setCurrentPage: (page: number) => void;
  setGroupBy: (groupBy: "state" | "priority" | "assignees") => void;
  toggleColumnExpand: (id: string) => void;
  setFilters: (filters: Partial<IIssueStore["filters"]>) => void;
  setSortBy: (field: string) => void;
  setSortDirection: (dir: "asc" | "desc") => void;
  clearFilters: () => void;
}

export class IssueStore implements IIssueStore {
  selectedIssueIds: string[] = [];
  activeView: "list" | "kanban" = "list";
  currentPage: number = 1;
  pageSize: number = 20;
  groupBy: "state" | "priority" | "assignees" = "state";
  expandedColumnIds: string[] = [];
  filters: IIssueStore["filters"] = {
    stateIds: [],
    priorityIds: [],
    assigneeIds: [],
    searchQuery: "",
  };
  sortBy: string = "updated_at";
  sortDirection: "asc" | "desc" = "desc";

  constructor() {
    makeObservable(this, {
      selectedIssueIds: observable,
      activeView: observable.ref,
      currentPage: observable.ref,
      pageSize: observable.ref,
      groupBy: observable.ref,
      expandedColumnIds: observable,
      filters: observable,
      sortBy: observable.ref,
      sortDirection: observable.ref,
      toggleIssueSelection: action,
      selectAll: action,
      clearSelection: action,
      setActiveView: action,
      setCurrentPage: action,
      setGroupBy: action,
      toggleColumnExpand: action,
      setFilters: action,
      setSortBy: action,
      setSortDirection: action,
      clearFilters: action,
    });
  }

  toggleIssueSelection = (id: string): void => {
    const idx = this.selectedIssueIds.indexOf(id);
    if (idx >= 0) {
      this.selectedIssueIds.splice(idx, 1);
    } else {
      this.selectedIssueIds.push(id);
    }
  };

  selectAll = (ids: string[]): void => {
    this.selectedIssueIds = ids;
  };

  clearSelection = (): void => {
    this.selectedIssueIds = [];
  };

  setActiveView = (view: "list" | "kanban"): void => {
    this.activeView = view;
  };

  setCurrentPage = (page: number): void => {
    this.currentPage = page;
  };

  setGroupBy = (groupBy: "state" | "priority" | "assignees"): void => {
    this.groupBy = groupBy;
  };

  toggleColumnExpand = (id: string): void => {
    const idx = this.expandedColumnIds.indexOf(id);
    if (idx >= 0) {
      this.expandedColumnIds.splice(idx, 1);
    } else {
      this.expandedColumnIds.push(id);
    }
  };

  setFilters = (filters: Partial<IIssueStore["filters"]>): void => {
    Object.assign(this.filters, filters);
  };

  setSortBy = (field: string): void => {
    this.sortBy = field;
  };

  setSortDirection = (dir: "asc" | "desc"): void => {
    this.sortDirection = dir;
  };

  clearFilters = (): void => {
    this.filters = { stateIds: [], priorityIds: [], assigneeIds: [], searchQuery: "" };
    this.currentPage = 1;
  };
}
