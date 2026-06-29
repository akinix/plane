// FLOW: useViewSwitcher — view switching hook for all 5 Issue views (per D-P17-05, D-P17-06)
import { useCallback, useMemo } from "react";
import { List, Kanban, CalendarDays, GitBranch, Table } from "lucide-react";
import { useStore } from "@/lib/store-context";
import type { TViewLayout } from "@/components/issues/filters/types";

export type TViewOption = {
  value: TViewLayout;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
};

export type TUseViewSwitcherReturn = {
  activeView: TViewLayout;
  viewOptions: TViewOption[];
  setActiveView: (view: TViewLayout) => void;
};

export function useViewSwitcher(): TUseViewSwitcherReturn {
  const store = useStore();

  const viewOptions: TViewOption[] = useMemo(
    () => [
      { value: "list", label: "列表", icon: List },
      { value: "kanban", label: "看板", icon: Kanban },
      { value: "calendar", label: "日历", icon: CalendarDays },
      { value: "gantt", label: "甘特", icon: GitBranch },
      { value: "spreadsheet", label: "表格", icon: Table },
    ],
    []
  );

  const setActiveView = useCallback(
    (view: TViewLayout) => {
      store.issue.setActiveView(view);
    },
    [store.issue]
  );

  return {
    activeView: store.issue.activeView,
    viewOptions,
    setActiveView,
  };
}
