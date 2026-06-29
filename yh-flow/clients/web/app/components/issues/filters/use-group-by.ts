// FLOW: useGroupBy — group-by logic shared across all 5 views (per D-P17-12)
import { useMemo, useState } from "react";
import type { TGroupByOptions } from "./types";
import type { TIssue } from "@plane/types";
import { MOCK_STATES } from "@/../src/lib/mock-data";

const PRIORITY_ORDER = ["urgent", "high", "medium", "low", "none"] as const;

type GroupedResult = {
  id: string;
  title: string;
  color?: string;
  issues: TIssue[];
};

function groupByState(issues: TIssue[], projectId: string): GroupedResult[] {
  const states = MOCK_STATES.filter((s) => s.project_id === projectId).toSorted((a, b) => a.order - b.order);
  return states.map((state) => ({
    id: state.id,
    title: state.name,
    color: state.color,
    issues: issues.filter((i) => i.state_id === state.id),
  }));
}

function groupByPriority(issues: TIssue[]): GroupedResult[] {
  const labels: Record<string, string> = { urgent: "紧急", high: "高", medium: "中", low: "低", none: "无" };
  return Array.from(PRIORITY_ORDER).map((priority) => ({
    id: priority,
    title: labels[priority] ?? priority,
    color: undefined,
    issues: issues.filter((i) => (i.priority ?? "none") === priority),
  }));
}

function groupByAssignees(issues: TIssue[]): GroupedResult[] {
  const assigneeIds = new Set<string>();
  issues.forEach((i) => {
    if (i.assignee_ids.length === 0) {
      assigneeIds.add("__unassigned__");
    } else {
      i.assignee_ids.forEach((aid) => assigneeIds.add(aid));
    }
  });
  return Array.from(assigneeIds).map((assigneeId) => {
    if (assigneeId === "__unassigned__") {
      return {
        id: "__unassigned__",
        title: "未指派",
        color: "#9CA3AF",
        issues: issues.filter((i) => i.assignee_ids.length === 0),
      };
    }
    return {
      id: assigneeId,
      title: assigneeId,
      color: undefined,
      issues: issues.filter((i) => i.assignee_ids.includes(assigneeId)),
    };
  });
}

function groupByCreatedBy(issues: TIssue[]): GroupedResult[] {
  const creators = new Set(issues.map((i) => i.created_by).filter(Boolean));
  return Array.from(creators).map((creatorId) => ({
    id: creatorId ?? "unknown",
    title: creatorId ?? "未知",
    color: undefined,
    issues: issues.filter((i) => i.created_by === creatorId),
  }));
}

export function groupIssues(issues: TIssue[], groupBy: TGroupByOptions, projectId?: string): GroupedResult[] {
  if (!issues) return [];
  switch (groupBy) {
    case "state":
      return groupByState(issues, projectId ?? "");
    case "priority":
      return groupByPriority(issues);
    case "assignees":
      return groupByAssignees(issues);
    case "created_by":
      return groupByCreatedBy(issues);
    case "none":
      return [{ id: "all", title: "全部", issues }];
    default:
      return [{ id: "all", title: "全部", issues }];
  }
}

type UseGroupByReturn = {
  groupBy: TGroupByOptions;
  setGroupBy: (g: TGroupByOptions) => void;
  groupedIssues: GroupedResult[];
};

export function useGroupBy(
  issues: TIssue[],
  initialGroupBy: TGroupByOptions = "state",
  projectId?: string
): UseGroupByReturn {
  const [groupBy, setGroupBy] = useState<TGroupByOptions>(initialGroupBy);

  const groupedIssues = useMemo(() => groupIssues(issues, groupBy, projectId), [issues, groupBy, projectId]);

  return { groupBy, setGroupBy, groupedIssues };
}
