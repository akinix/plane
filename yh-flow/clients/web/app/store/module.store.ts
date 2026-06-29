// FLOW: ModuleStore — MobX UI state for Module views (per D-P18-05)
import { action, makeObservable, observable } from "mobx";
import type { TModuleStatus } from "@plane/types";

export interface IModuleStore {
  // UI State
  selectedModuleId: string | null;
  moduleModalOpen: boolean;
  moduleModalMode: "create" | "edit";
  moduleDeleting: boolean;
  activeView: "list" | "gantt";
  filters: {
    status?: TModuleStatus[];
    searchQuery?: string;
  };

  // Actions
  setSelectedModuleId: (id: string | null) => void;
  openModuleModal: (mode: "create" | "edit") => void;
  closeModuleModal: () => void;
  setModuleDeleting: (state: boolean) => void;
  setActiveView: (view: "list" | "gantt") => void;
  setFilters: (filters: { status?: TModuleStatus[]; searchQuery?: string }) => void;
  reset: () => void;
}

export class ModuleStore implements IModuleStore {
  selectedModuleId: string | null = null;
  moduleModalOpen: boolean = false;
  moduleModalMode: "create" | "edit" = "create";
  moduleDeleting: boolean = false;
  activeView: "list" | "gantt" = "list";
  filters: {
    status?: TModuleStatus[];
    searchQuery?: string;
  } = {};

  constructor() {
    makeObservable(this, {
      selectedModuleId: observable.ref,
      moduleModalOpen: observable.ref,
      moduleModalMode: observable.ref,
      moduleDeleting: observable.ref,
      activeView: observable.ref,
      filters: observable,
      setSelectedModuleId: action,
      openModuleModal: action,
      closeModuleModal: action,
      setModuleDeleting: action,
      setActiveView: action,
      setFilters: action,
      reset: action,
    });
  }

  setSelectedModuleId = (id: string | null): void => {
    this.selectedModuleId = id;
  };

  openModuleModal = (mode: "create" | "edit"): void => {
    this.moduleModalMode = mode;
    this.moduleModalOpen = true;
  };

  closeModuleModal = (): void => {
    this.moduleModalOpen = false;
  };

  setModuleDeleting = (state: boolean): void => {
    this.moduleDeleting = state;
  };

  setActiveView = (view: "list" | "gantt"): void => {
    this.activeView = view;
  };

  setFilters = (filters: { status?: TModuleStatus[]; searchQuery?: string }): void => {
    Object.assign(this.filters, filters);
  };

  reset = (): void => {
    this.selectedModuleId = null;
    this.moduleModalOpen = false;
    this.moduleModalMode = "create";
    this.moduleDeleting = false;
    this.activeView = "list";
    this.filters = {};
  };
}
