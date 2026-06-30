// FLOW: AnalyticsStore — MobX UI state for workspace analytics dashboard (per D-P20-06)
import { action, makeObservable, observable } from "mobx";

export type TAnalyticsTab = "overview" | "work-items";

export interface IAnalyticsStore {
  // State
  activeTab: TAnalyticsTab;
  selectedProjectIds: string[];
  dateRange: string;

  // Actions
  setActiveTab: (tab: TAnalyticsTab) => void;
  setSelectedProjectIds: (ids: string[]) => void;
  setDateRange: (range: string) => void;
  reset: () => void;
}

export class AnalyticsStore implements IAnalyticsStore {
  activeTab: TAnalyticsTab = "overview";
  selectedProjectIds: string[] = [];
  dateRange: string = "last-30-days";

  constructor() {
    makeObservable(this, {
      activeTab: observable.ref,
      selectedProjectIds: observable.ref,
      dateRange: observable.ref,
      setActiveTab: action,
      setSelectedProjectIds: action,
      setDateRange: action,
      reset: action,
    });
  }

  setActiveTab = (tab: TAnalyticsTab): void => {
    this.activeTab = tab;
  };

  setSelectedProjectIds = (ids: string[]): void => {
    this.selectedProjectIds = ids;
  };

  setDateRange = (range: string): void => {
    this.dateRange = range;
  };

  reset = (): void => {
    this.activeTab = "overview";
    this.selectedProjectIds = [];
    this.dateRange = "last-30-days";
  };
}
