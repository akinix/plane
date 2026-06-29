// FLOW: IssuesKanbanView — Kanban board container (DragDropContext + column layout + GroupBySelector)
// per D-P16-13 (DragDropContext), D-P16-14 (GroupBy state/priority/assignees)
import { useRef, useMemo, useCallback } from "react";
import { observer } from "mobx-react";
import { Kanban } from "lucide-react";
import { cn } from "@plane/utils";
import { DragDropContext, type DropResult } from "@hello-pangea/dnd";
import { useStore } from "@/lib/store-context";
import { useIssues, useIssueMutations } from "@/../src/lib/hooks/use-issues";
import { MOCK_STATES, MOCK_PROJECTS } from "@/../src/lib/mock-data";
import { Loader } from "@plane/ui";
import { KanbanColumn } from "./kanban-column";
import type { TIssue } from "@plane/types";

type GroupByType = "state" | "priority" | "assignees";

const PRIORITY_ORDER = ["urgent", "high", "medium", "low", "none"] as const;

const PRIORITY_LABELS: Record<string, string> = {
  urgent: "紧急",
  high: "高",
  medium: "中",
  low: "低",
  none: "无",
};

const PRIORITY_COLORS: Record<string, string> = {
  urgent: "#D1453B",
  high: "#D97706",
  medium: "#EAB308",
  low: "#3B82F6",
  none: "#9CA3AF",
};

const ASSIGNEE_NAMES: Record<string, string> = {
  "user-1": "张三",
  "user-2": "李四",
  "user-3": "王五",
  "user-4": "赵六",
  "user-5": "陈七",
};

const GROUP_BY_OPTIONS: { value: GroupByType; label: string }[] = [
  { value: "state", label: "按状态" },
  { value: "priority", label: "按优先级" },
  { value: "assignees", label: "按负责人" },
];

type TProps = {
  workspaceId: string;
  projectId: string;
};

const IssuesKanbanView = observer(function IssuesKanbanView({ workspaceId, projectId }: TProps) {
  const store = useStore();
  const { updateIssue } = useIssueMutations();
  const dragHandledRef = useRef(false);

  const { data: issues, isLoading } = useIssues(projectId, {
    state: store.issue.filters.stateIds,
    priority: store.issue.filters.priorityIds,
    assignee: store.issue.filters.assigneeIds,
    searchQuery: store.issue.filters.searchQuery,
  });

  const groupBy = store.issue.groupBy;

  // Resolve project identifier for issue ID display
  const projectIdentifier = useMemo(
    () => MOCK_PROJECTS.find((p) => p.id === projectId)?.identifier ?? "",
    [projectId]
  );

  // Build columns based on groupBy mode
  const columns = useMemo(() => {
    if (!issues) return [];

    if (groupBy === "state") {
      const states = MOCK_STATES.filter((s) => s.project_id === projectId).sort((a, b) => a.order - b.order);
      return states.map((state) => ({
        id: state.id,
        title: state.name,
        color: state.color,
        issues: issues.filter((i) => i.state_id === state.id),
      }));
    }

    if (groupBy === "priority") {
      return PRIORITY_ORDER.map((priority) => ({
        id: priority,
        title: PRIORITY_LABELS[priority] ?? priority,
        color: PRIORITY_COLORS[priority] ?? "#9CA3AF",
        issues: issues.filter((i) => (i.priority ?? "none") === priority),
      }));
    }

    if (groupBy === "assignees") {
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
          title: ASSIGNEE_NAMES[assigneeId] ?? assigneeId,
          color: "#6B7280",
          issues: issues.filter((i) => i.assignee_ids.includes(assigneeId)),
        };
      });
    }

    return [];
  }, [issues, groupBy, projectId]);

  // Build update payload based on groupBy mode
  const getUpdatePayload = useCallback(
    (draggableId: string, destinationDroppableId: string): Partial<TIssue> => {
      if (groupBy === "state") {
        return { state_id: destinationDroppableId };
      }
      if (groupBy === "priority") {
        return { priority: destinationDroppableId as TIssue["priority"] };
      }
      if (groupBy === "assignees") {
        return { assignee_ids: [destinationDroppableId] };
      }
      return {};
    },
    [groupBy]
  );

  const onDragEnd = useCallback(
    (result: DropResult) => {
      // Prevent StrictMode double-fire (Pitfall 5 from RESEARCH.md)
      if (dragHandledRef.current) return;

      const { draggableId, destination, source } = result;
      if (!destination) return;
      if (destination.droppableId === source.droppableId && destination.index === source.index) return;

      dragHandledRef.current = true;
      setTimeout(() => {
        dragHandledRef.current = false;
      }, 300);

      const payload = getUpdatePayload(draggableId, destination.droppableId);
      updateIssue.mutate({ issueId: draggableId, data: payload });
    },
    [updateIssue, getUpdatePayload]
  );

  const handleGroupByChange = useCallback(
    (value: GroupByType) => {
      store.issue.setGroupBy(value);
    },
    [store.issue]
  );

  const hasActiveFilters =
    store.issue.filters.stateIds.length > 0 ||
    store.issue.filters.priorityIds.length > 0 ||
    store.issue.filters.assigneeIds.length > 0 ||
    store.issue.filters.searchQuery.length > 0;

  // Loading state: 5 skeleton columns (per UI-SPEC.md)
  if (isLoading) {
    return (
      <div className="flex h-full flex-col">
        <div className="flex items-center gap-4 px-6 py-3">
          <Loader className="flex items-center gap-2">
            <Loader.Item width="120px" height="32px" className="rounded-md" />
          </Loader>
        </div>
        <div className="flex gap-4 overflow-x-auto px-6 pb-4">
          {Array.from({ length: 5 }).map((_, i) => (
            <div
              key={i}
              className="flex w-[280px] shrink-0 flex-col gap-3 rounded-lg bg-custom-background-80 p-3"
            >
              <Loader className="space-y-3">
                <Loader.Item width="100px" height="20px" className="rounded" />
                {Array.from({ length: 3 }).map((__, j) => (
                  <div key={j} className="rounded-md bg-custom-background-100 p-3">
                    <Loader.Item width="100%" height="14px" className="rounded" />
                    <Loader.Item width="60%" height="14px" className="mt-2 rounded" />
                    <Loader.Item width="40px" height="20px" className="mt-2 rounded" />
                  </div>
                ))}
              </Loader>
            </div>
          ))}
        </div>
      </div>
    );
  }

  // Empty state — no issues at all (per UI-SPEC.md Copywriting Contract)
  if (!issues || issues.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16">
        <div className="mb-3 rounded-full bg-custom-background-80 p-4">
          <Kanban className="size-8 text-custom-text-400" />
        </div>
        <h3 className="text-sm font-medium text-custom-text-200">暂无 Issue</h3>
        <p className="mt-1 text-xs text-custom-text-400">这个项目还没有创建 Issue。</p>
      </div>
    );
  }

  return (
    <div className="flex h-full flex-col">
      {/* GroupBy selector bar */}
      <div className="flex items-center gap-4 border-b border-custom-border-200 px-6 py-3">
        <div className="flex items-center gap-2 rounded-md border border-custom-border-200 bg-custom-background-90 p-0.5">
          <Kanban className="ml-2 size-3.5 text-custom-text-400" />
          {GROUP_BY_OPTIONS.map((option) => (
            <button
              key={option.value}
              type="button"
              onClick={() => handleGroupByChange(option.value)}
              className={cn(
                "rounded-sm px-2.5 py-1 text-xs transition-colors",
                groupBy === option.value
                  ? "bg-custom-background-100 text-custom-text-100 shadow-sm"
                  : "text-custom-text-400 hover:text-custom-text-200"
              )}
            >
              {option.label}
            </button>
          ))}
        </div>

        {/* Active filter indicator */}
        {hasActiveFilters && (
          <span className="text-xs text-custom-text-400">筛选条件已激活</span>
        )}
      </div>

      {/* Kanban board — horizontal scrollable */}
      <DragDropContext onDragEnd={onDragEnd}>
        <div className="flex gap-4 overflow-x-auto px-6 pb-4 pt-3" style={{ minHeight: "320px" }}>
          {columns.length === 0 && (
            <div className="flex w-full items-center justify-center py-16">
              <div className="flex flex-col items-center gap-2">
                <Kanban className="size-8 text-custom-text-400" />
                <h3 className="text-sm font-medium text-custom-text-200">没有匹配的 Issue</h3>
                <p className="text-xs text-custom-text-400">没有符合当前分组条件的 Issue。</p>
                <button
                  type="button"
                  onClick={() => store.issue.clearFilters()}
                  className="mt-1 text-xs text-custom-primary hover:underline"
                >
                  清除所有筛选条件
                </button>
              </div>
            </div>
          )}
          {columns.map((col) => {
            const isExpanded = store.issue.expandedColumnIds.includes(col.id);
            return (
              <KanbanColumn
                key={col.id}
                columnId={col.id}
                title={col.title}
                color={col.color}
                issues={col.issues}
                isExpanded={isExpanded}
                onToggleExpand={() => store.issue.toggleColumnExpand(col.id)}
                index={0}
                workspaceId={workspaceId}
                projectId={projectId}
                projectIdentifier={projectIdentifier}
              />
            );
          })}
        </div>
      </DragDropContext>
    </div>
  );
});

export { IssuesKanbanView };
