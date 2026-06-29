// FLOW: IssuesKanbanView — Kanban board container (DragDropContext + column layout + GroupBy + SubGroup)
// per D-P16-13 (DragDropContext), D-P16-14 (GroupBy), D-P17-06 (FilterBar + SubGroup KANB-04)
import { useRef, useMemo, useCallback } from "react";
import { observer } from "mobx-react";
import { Kanban, ChevronDown, ChevronRight } from "lucide-react";
import { cn } from "@plane/utils";
import { DragDropContext, Droppable, type DropResult } from "@hello-pangea/dnd";
import { useStore } from "@/lib/store-context";
import { useIssues, useIssueMutations } from "@/../src/lib/hooks/use-issues";
import { MOCK_STATES, MOCK_PROJECTS } from "@/../src/lib/mock-data";
import { Loader } from "@plane/ui";
import { FilterBar } from "./filters/filter-bar";
import { subGroupIssues, useSubGroupBy } from "./filters/use-sub-group-by";
import { KanbanColumn } from "./kanban-column";
import { KanbanCard } from "./kanban-card";
import type { TIssue } from "@plane/types";
import type { TGroupByOptions, TSubGroupByOptions } from "./filters/types";

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

const SUB_GROUP_OPTIONS: { value: TSubGroupByOptions; label: string }[] = [
  { value: "none", label: "无" },
  { value: "state", label: "按状态" },
  { value: "priority", label: "按优先级" },
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

  // Sub-group (swimlane) state
  const { subGroupBy, setSubGroupBy } = useSubGroupBy(issues ?? [], "none", projectId);

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
      store.issue.setGroupBy(value as TGroupByOptions);
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
      {/* FilterBar + GroupBy + SubGroup header */}
      <div className="border-b border-custom-border-200 px-6 py-3">
        <FilterBar
          workspaceId={workspaceId}
          projectId={projectId}
          viewType="kanban"
          showSorting={false}
          showGroupBy={true}
        />

        {/* Sub-group (swimlane) selector */}
        <div className="mt-2 flex items-center gap-4">
          {/* GroupBy selector (kept alongside FilterBar) */}
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

          {/* Sub-group dropdown */}
          <div className="flex items-center gap-2 rounded-md border border-custom-border-200 bg-custom-background-90 p-0.5">
            <span className="ml-2 text-xs text-custom-text-400">子分组：</span>
            {SUB_GROUP_OPTIONS.map((option) => (
              <button
                key={option.value}
                type="button"
                onClick={() => setSubGroupBy(option.value)}
                className={cn(
                  "rounded-sm px-2.5 py-1 text-xs transition-colors",
                  subGroupBy === option.value
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

            // Sub-group (swimlane) rendering
            if (subGroupBy !== "none") {
              const subGroups = subGroupIssues(col.issues, subGroupBy, projectId);
              return (
                <div
                  key={col.id}
                  className="flex w-[280px] shrink-0 flex-col rounded-lg bg-custom-background-80"
                  style={{ minHeight: "80px" }}
                >
                  {/* Column header */}
                  <div className="sticky top-0 z-10 flex items-center justify-between rounded-t-lg bg-custom-background-90 px-3 py-2.5">
                    <div className="flex items-center gap-2 min-w-0">
                      <span
                        className="inline-block size-2.5 shrink-0 rounded-full"
                        style={{ backgroundColor: col.color }}
                      />
                      <span className="truncate text-xs font-medium text-custom-text-200">{col.title}</span>
                      <span className="inline-flex size-5 shrink-0 items-center justify-center rounded-full bg-custom-background-80 text-[10px] text-custom-text-400">
                        {col.issues.length}
                      </span>
                    </div>
                    <button
                      type="button"
                      onClick={() => store.issue.toggleColumnExpand(col.id)}
                      className="flex size-5 shrink-0 items-center justify-center rounded text-custom-text-400 hover:bg-custom-background-80 hover:text-custom-text-200 transition-colors"
                      aria-label={isExpanded ? "折叠列" : "展开列"}
                    >
                      {isExpanded ? <ChevronDown className="size-3.5" /> : <ChevronRight className="size-3.5" />}
                    </button>
                  </div>

                  {/* Column body with swimlanes */}
                  {isExpanded && (
                    <Droppable droppableId={col.id}>
                      {(provided, snapshot) => (
                        <div
                          ref={provided.innerRef}
                          {...provided.droppableProps}
                          className={cn(
                            "flex flex-col gap-0 px-2 pb-2 pt-1",
                            snapshot.isDraggingOver && "bg-custom-background-90 ring-2 ring-custom-primary/20"
                          )}
                        >
                          {subGroups.map((sg) => (
                            <div key={sg.id}>
                              {/* Sub-group header / swimlane label */}
                              <div className="flex items-center gap-1.5 px-1 py-1.5">
                                {sg.color && (
                                  <span
                                    className="inline-block size-2 shrink-0 rounded-full"
                                    style={{ backgroundColor: sg.color }}
                                  />
                                )}
                                <span className="text-[10px] font-medium text-custom-text-400">{sg.title}</span>
                                <span className="text-[10px] text-custom-text-500">{sg.issues.length}</span>
                              </div>

                              {/* Swimlane cards */}
                              {sg.issues.map((issue, idx) => (
                                <KanbanCard
                                  key={issue.id}
                                  issue={issue}
                                  index={idx}
                                  columnId={col.id}
                                  workspaceId={workspaceId}
                                  projectId={projectId}
                                  projectIdentifier={projectIdentifier}
                                />
                              ))}

                              {/* Empty swimlane */}
                              {sg.issues.length === 0 && !snapshot.isDraggingOver && (
                                <div className="flex items-center justify-center py-4">
                                  <p className="text-[10px] text-custom-text-400">拖动 Issue 到此</p>
                                </div>
                              )}
                            </div>
                          ))}
                          {provided.placeholder}
                        </div>
                      )}
                    </Droppable>
                  )}

                  {/* Collapsed column shows just count */}
                  {!isExpanded && (
                    <div className="flex items-center justify-center py-8">
                      <p className="text-[10px] text-custom-text-400">{col.issues.length} 个 Issue（已折叠）</p>
                    </div>
                  )}
                </div>
              );
            }

            // Without sub-group, use standard KanbanColumn
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
