// FLOW: useSubGroupBy — sub-group/swimlane logic for Kanban view (KANB-04)
import { useMemo, useState } from "react";
import type { TSubGroupByOptions } from "./types";
import type { TIssue } from "@plane/types";
import { MOCK_STATES } from "@/../src/lib/mock-data";

type SubGroupedResult = {
  id: string;
  title: string;
  color?: string;
  issues: TIssue[];
};

function subGroupByState(issues: TIssue[], projectId: string): SubGroupedResult[] {
  const states = MOCK_STATES.filter((s) => s.project_id === projectId).toSorted((a, b) => a.order - b.order);
  return states.map((state) => ({
    id: state.id,
    title: state.name,
    color: state.color,
    issues: issues.filter((i) => i.state_id === state.id),
  }));
}

function subGroupByPriority(issues: TIssue[]): SubGroupedResult[] {
  const labels: Record<string, string> = { urgent: "紧急", high: "高", medium: "中", low: "低", none: "无" };
  const order = ["urgent", "high", "medium", "low", "none"] as const;
  return Array.from(order).map((priority) => ({
    id: priority,
    title: labels[priority] ?? priority,
    issues: issues.filter((i) => (i.priority ?? "none") === priority),
  }));
}

export function subGroupIssues(
  issues: TIssue[],
  subGroupBy: TSubGroupByOptions,
  projectId?: string
): SubGroupedResult[] {
  if (!issues) return [];
  switch (subGroupBy) {
    case "state":
      return subGroupByState(issues, projectId ?? "");
    case "priority":
      return subGroupByPriority(issues);
    case "none":
      return [{ id: "all", title: "全部", issues }];
    default:
      return [{ id: "all", title: "全部", issues }];
  }
}

type UseSubGroupByReturn = {
  subGroupBy: TSubGroupByOptions;
  setSubGroupBy: (sg: TSubGroupByOptions) => void;
  subGroupedIssues: SubGroupedResult[];
};

export function useSubGroupBy(
  issues: TIssue[],
  initialSubGroupBy: TSubGroupByOptions = "none",
  projectId?: string
): UseSubGroupByReturn {
  const [subGroupBy, setSubGroupBy] = useState<TSubGroupByOptions>(initialSubGroupBy);

  const subGroupedIssues = useMemo(
    () => subGroupIssues(issues, subGroupBy, projectId),
    [issues, subGroupBy, projectId]
  );

  return { subGroupBy, setSubGroupBy, subGroupedIssues };
}
