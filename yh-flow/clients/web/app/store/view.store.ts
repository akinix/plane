// FLOW: ViewStore — MobX UI state for View pages (per D-P19-09)
import { action, makeObservable, observable } from "mobx";
import type { TViewFilters, TViewFiltersSortKey, TViewFiltersSortBy } from "@/../src/lib/types/views";

export interface IViewStore {
  // UI State
  activeTab: "all" | "created";
  selectedViewId: string | null;
  viewModalOpen: boolean;
  viewModalMode: "create" | "edit";
  deleteViewId: string | null;
  deleteViewName: string | null;
  viewDeleting: boolean;
  appliedViewId: string | null;
  filters: TViewFilters;

  // Actions
  setActiveTab: (tab: "all" | "created") => void;
  setSelectedViewId: (id: string | null) => void;
  openViewModal: (mode: "create" | "edit") => void;
  closeViewModal: () => void;
  openDeleteModal: (viewId: string, viewName: string) => void;
  closeDeleteModal: () => void;
  setViewDeleting: (state: boolean) => void;
  setAppliedViewId: (id: string | null) => void;
  setSearchQuery: (query: string) => void;
  setSortKey: (key: TViewFiltersSortKey) => void;
  setSortBy: (by: TViewFiltersSortBy) => void;
  reset: () => void;
}

export class ViewStore implements IViewStore {
  activeTab: "all" | "created" = "all";
  selectedViewId: string | null = null;
  viewModalOpen: boolean = false;
  viewModalMode: "create" | "edit" = "create";
  deleteViewId: string | null = null;
  deleteViewName: string | null = null;
  viewDeleting: boolean = false;
  appliedViewId: string | null = null;
  filters: TViewFilters = {
    searchQuery: "",
    sortKey: "name",
    sortBy: "desc",
  };

  constructor() {
    makeObservable(this, {
      activeTab: observable.ref,
      selectedViewId: observable.ref,
      viewModalOpen: observable.ref,
      viewModalMode: observable.ref,
      deleteViewId: observable.ref,
      deleteViewName: observable.ref,
      viewDeleting: observable.ref,
      appliedViewId: observable.ref,
      filters: observable,
      setActiveTab: action,
      setSelectedViewId: action,
      openViewModal: action,
      closeViewModal: action,
      openDeleteModal: action,
      closeDeleteModal: action,
      setViewDeleting: action,
      setAppliedViewId: action,
      setSearchQuery: action,
      setSortKey: action,
      setSortBy: action,
      reset: action,
    });
  }

  setActiveTab = (tab: "all" | "created"): void => {
    this.activeTab = tab;
  };

  setSelectedViewId = (id: string | null): void => {
    this.selectedViewId = id;
  };

  openViewModal = (mode: "create" | "edit"): void => {
    this.viewModalMode = mode;
    this.viewModalOpen = true;
  };

  closeViewModal = (): void => {
    this.viewModalOpen = false;
  };

  openDeleteModal = (viewId: string, viewName: string): void => {
    this.deleteViewId = viewId;
    this.deleteViewName = viewName;
    this.viewDeleting = true;
  };

  closeDeleteModal = (): void => {
    this.deleteViewId = null;
    this.deleteViewName = null;
    this.viewDeleting = false;
  };

  setViewDeleting = (state: boolean): void => {
    this.viewDeleting = state;
  };

  setAppliedViewId = (id: string | null): void => {
    this.appliedViewId = id;
  };

  setSearchQuery = (query: string): void => {
    this.filters.searchQuery = query;
  };

  setSortKey = (key: TViewFiltersSortKey): void => {
    this.filters.sortKey = key;
  };

  setSortBy = (by: TViewFiltersSortBy): void => {
    this.filters.sortBy = by;
  };

  reset = (): void => {
    this.activeTab = "all";
    this.selectedViewId = null;
    this.viewModalOpen = false;
    this.viewModalMode = "create";
    this.deleteViewId = null;
    this.deleteViewName = null;
    this.viewDeleting = false;
    this.appliedViewId = null;
    this.filters = {
      searchQuery: "",
      sortKey: "name",
      sortBy: "desc",
    };
  };
}
