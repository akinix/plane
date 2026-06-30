// FLOW: PageStore — MobX UI state for Page views (per D-P19-09)
import { action, makeObservable, observable } from "mobx";
import type { TPageNavigationTabs, TPageFilters, TPageFiltersSortKey, TPageFiltersSortBy } from "@plane/types";

export interface IPageStore {
  // UI State
  activeTab: TPageNavigationTabs;
  selectedPageId: string | null;
  pageModalOpen: boolean;
  pageModalMode: "create" | "edit";
  deletePageId: string | null;
  deletePageName: string | null;
  pageDeleting: boolean;
  archivePageId: string | null;
  archivePageName: string | null;
  filters: TPageFilters;
  appliedViewId: string | null;

  // Actions
  setActiveTab: (tab: TPageNavigationTabs) => void;
  setSelectedPageId: (id: string | null) => void;
  openPageModal: (mode: "create" | "edit") => void;
  closePageModal: () => void;
  openDeleteModal: (pageId: string, pageName: string) => void;
  closeDeleteModal: () => void;
  setPageDeleting: (state: boolean) => void;
  openArchiveModal: (pageId: string, pageName: string) => void;
  closeArchiveModal: () => void;
  setFilters: (filters: Partial<TPageFilters>) => void;
  setSortKey: (key: TPageFiltersSortKey) => void;
  setSortBy: (by: TPageFiltersSortBy) => void;
  setSearchQuery: (query: string) => void;
  setAppliedViewId: (id: string | null) => void;
  reset: () => void;
}

export class PageStore implements IPageStore {
  activeTab: TPageNavigationTabs = "public";
  selectedPageId: string | null = null;
  pageModalOpen: boolean = false;
  pageModalMode: "create" | "edit" = "create";
  deletePageId: string | null = null;
  deletePageName: string | null = null;
  pageDeleting: boolean = false;
  archivePageId: string | null = null;
  archivePageName: string | null = null;
  filters: TPageFilters = {
    searchQuery: "",
    sortKey: "created_at",
    sortBy: "desc",
  };
  appliedViewId: string | null = null;

  constructor() {
    makeObservable(this, {
      activeTab: observable.ref,
      selectedPageId: observable.ref,
      pageModalOpen: observable.ref,
      pageModalMode: observable.ref,
      deletePageId: observable.ref,
      deletePageName: observable.ref,
      pageDeleting: observable.ref,
      archivePageId: observable.ref,
      archivePageName: observable.ref,
      filters: observable,
      appliedViewId: observable.ref,
      setActiveTab: action,
      setSelectedPageId: action,
      openPageModal: action,
      closePageModal: action,
      openDeleteModal: action,
      closeDeleteModal: action,
      setPageDeleting: action,
      openArchiveModal: action,
      closeArchiveModal: action,
      setFilters: action,
      setSortKey: action,
      setSortBy: action,
      setSearchQuery: action,
      setAppliedViewId: action,
      reset: action,
    });
  }

  setActiveTab = (tab: TPageNavigationTabs): void => {
    this.activeTab = tab;
  };

  setSelectedPageId = (id: string | null): void => {
    this.selectedPageId = id;
  };

  openPageModal = (mode: "create" | "edit"): void => {
    this.pageModalMode = mode;
    this.pageModalOpen = true;
  };

  closePageModal = (): void => {
    this.pageModalOpen = false;
  };

  openDeleteModal = (pageId: string, pageName: string): void => {
    this.deletePageId = pageId;
    this.deletePageName = pageName;
    this.pageDeleting = true;
  };

  closeDeleteModal = (): void => {
    this.deletePageId = null;
    this.deletePageName = null;
    this.pageDeleting = false;
  };

  setPageDeleting = (state: boolean): void => {
    this.pageDeleting = state;
  };

  openArchiveModal = (pageId: string, pageName: string): void => {
    this.archivePageId = pageId;
    this.archivePageName = pageName;
  };

  closeArchiveModal = (): void => {
    this.archivePageId = null;
    this.archivePageName = null;
  };

  setFilters = (filters: Partial<TPageFilters>): void => {
    Object.assign(this.filters, filters);
  };

  setSortKey = (key: TPageFiltersSortKey): void => {
    this.filters.sortKey = key;
  };

  setSortBy = (by: TPageFiltersSortBy): void => {
    this.filters.sortBy = by;
  };

  setSearchQuery = (query: string): void => {
    this.filters.searchQuery = query;
  };

  setAppliedViewId = (id: string | null): void => {
    this.appliedViewId = id;
  };

  reset = (): void => {
    this.activeTab = "public";
    this.selectedPageId = null;
    this.pageModalOpen = false;
    this.pageModalMode = "create";
    this.deletePageId = null;
    this.deletePageName = null;
    this.pageDeleting = false;
    this.archivePageId = null;
    this.archivePageName = null;
    this.filters = {
      searchQuery: "",
      sortKey: "created_at",
      sortBy: "desc",
    };
    this.appliedViewId = null;
  };
}
