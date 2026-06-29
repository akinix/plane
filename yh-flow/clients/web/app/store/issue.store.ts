// FLOW: IssueStore — MobX UI state for Issue views (per D-P16-02)
// Extended for Phase 17: 5 view layouts, visibleColumnIds, TGroupByOptions
import { action, makeObservable, observable } from "mobx";
import type { TViewLayout, TGroupByOptions, TFilterCriteria } from "@/components/issues/filters/types";

export interface IIssueStore {
  selectedIssueIds: string[];
  activeView: TViewLayout;
  currentPage: number;
  pageSize: number;
  groupBy: TGroupByOptions;
  expandedColumnIds: string[];
  visibleColumnIds: string[];
  filters: TFilterCriteria;
  sortBy: string;
  sortDirection: "asc" | "desc";

  toggleIssueSelection: (id: string) => void;
  selectAll: (ids: string[]) => void;
  clearSelection: () => void;
  setActiveView: (view: TViewLayout) => void;
  setCurrentPage: (page: number) => void;
  setGroupBy: (groupBy: TGroupByOptions) => void;
  toggleColumnExpand: (id: string) => void;
  setFilters: (filters: Partial<TFilterCriteria>) => void;
  setSortBy: (field: string) => void;
  setSortDirection: (dir: "asc" | "desc") => void;
  clearFilters: () => void;
  setVisibleColumnIds: (ids: string[]) => void;
}

export class IssueStore implements IIssueStore {
  selectedIssueIds: string[] = [];
  activeView: TViewLayout = "list";
  currentPage: number = 1;
  pageSize: number = 20;
  groupBy: TGroupByOptions = "state";
  expandedColumnIds: string[] = [];
  visibleColumnIds: string[] = [];
  filters: TFilterCriteria = {
    stateIds: [],
    priorityIds: [],
    assigneeIds: [],
    labelIds: [],
    searchQuery: "",
    dateRange: null,
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
      visibleColumnIds: observable,
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
      setVisibleColumnIds: action,
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

  setActiveView = (view: TViewLayout): void => {
    this.activeView = view;
  };

  setCurrentPage = (page: number): void => {
    this.currentPage = page;
  };

  setGroupBy = (groupBy: TGroupByOptions): void => {
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

  setFilters = (filters: Partial<TFilterCriteria>): void => {
    Object.assign(this.filters, filters);
  };

  setSortBy = (field: string): void => {
    this.sortBy = field;
  };

  setSortDirection = (dir: "asc" | "desc"): void => {
    this.sortDirection = dir;
  };

  clearFilters = (): void => {
    this.filters = {
      stateIds: [],
      priorityIds: [],
      assigneeIds: [],
      labelIds: [],
      searchQuery: "",
      dateRange: null,
    };
    this.currentPage = 1;
  };

  setVisibleColumnIds = (ids: string[]): void => {
    this.visibleColumnIds = ids;
  };
}
