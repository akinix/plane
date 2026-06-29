// FLOW: CycleStore — MobX UI state for Cycle views (per D-P18-04)
import { action, makeObservable, observable } from "mobx";
export interface ICycleStore {
  // UI State
  activeTab: "active" | "completed" | "all";
  selectedCycleId: string | null;
  viewLayout: "list" | "board";
  cycleModalOpen: boolean;
  cycleModalMode: "create" | "edit";
  cycleDeleting: boolean;
  transferModalOpen: boolean;

  // Actions
  setActiveTab: (tab: "active" | "completed" | "all") => void;
  setSelectedCycleId: (id: string | null) => void;
  setViewLayout: (layout: "list" | "board") => void;
  openCycleModal: (mode: "create" | "edit") => void;
  closeCycleModal: () => void;
  setCycleDeleting: (state: boolean) => void;
  setTransferModalOpen: (state: boolean) => void;
  reset: () => void;
}

export class CycleStore implements ICycleStore {
  activeTab: "active" | "completed" | "all" = "active";
  selectedCycleId: string | null = null;
  viewLayout: "list" | "board" = "list";
  cycleModalOpen: boolean = false;
  cycleModalMode: "create" | "edit" = "create";
  cycleDeleting: boolean = false;
  transferModalOpen: boolean = false;

  constructor() {
    makeObservable(this, {
      activeTab: observable.ref,
      selectedCycleId: observable.ref,
      viewLayout: observable.ref,
      cycleModalOpen: observable.ref,
      cycleModalMode: observable.ref,
      cycleDeleting: observable.ref,
      transferModalOpen: observable.ref,
      setActiveTab: action,
      setSelectedCycleId: action,
      setViewLayout: action,
      openCycleModal: action,
      closeCycleModal: action,
      setCycleDeleting: action,
      setTransferModalOpen: action,
      reset: action,
    });
  }

  setActiveTab = (tab: "active" | "completed" | "all"): void => {
    this.activeTab = tab;
  };

  setSelectedCycleId = (id: string | null): void => {
    this.selectedCycleId = id;
  };

  setViewLayout = (layout: "list" | "board"): void => {
    this.viewLayout = layout;
  };

  openCycleModal = (mode: "create" | "edit"): void => {
    this.cycleModalMode = mode;
    this.cycleModalOpen = true;
  };

  closeCycleModal = (): void => {
    this.cycleModalOpen = false;
  };

  setCycleDeleting = (state: boolean): void => {
    this.cycleDeleting = state;
  };

  setTransferModalOpen = (state: boolean): void => {
    this.transferModalOpen = state;
  };

  reset = (): void => {
    this.activeTab = "active";
    this.selectedCycleId = null;
    this.viewLayout = "list";
    this.cycleModalOpen = false;
    this.cycleModalMode = "create";
    this.cycleDeleting = false;
    this.transferModalOpen = false;
  };
}
