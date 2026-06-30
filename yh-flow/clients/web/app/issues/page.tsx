// FLOW: Issue list page — entry point for /workspaces/:wsId/projects/:projId/issues (per D-P16-01)
// Phase 17: 5-view toggle with lazy loading for calendar/gantt/spreadsheet
import { useState, lazy, Suspense, useCallback } from "react";
import { useParams, useNavigate } from "react-router";
import { observer } from "mobx-react";
import { List, Kanban, CalendarDays, GitBranch as GanttIcon, Table, Plus } from "lucide-react";
import { cn } from "@plane/utils";
import { useStore } from "@/lib/store-context";
import { useIssues } from "@/../src/lib/hooks/use-issues";
import { IssueListView } from "@/components/issues/list-view";
import { IssueCreateModal } from "@/components/issues/issue-create-modal";

// Lazy-loaded views (4 views: kanban, calendar, gantt, spreadsheet)
const IssuesKanbanView = lazy(() =>
  import("@/components/issues/kanban-view").then((m) => ({ default: m.IssuesKanbanView }))
);

const IssuesCalendarView = lazy(() => import("@/components/issues/calendar-view/calendar-view"));

const IssuesGanttView = lazy(() =>
  import("@/components/issues/gantt-view/gantt-view").then((m) => ({ default: m.GanttView }))
);

const IssuesSpreadsheetView = lazy(() =>
  import("@/components/issues/spreadsheet-view/base-spreadsheet-root").then((m) => ({
    default: m.BaseSpreadsheetRoot,
  }))
);

// View toggle configuration
const VIEW_OPTIONS = [
  { value: "list", label: "列表", icon: List },
  { value: "kanban", label: "看板", icon: Kanban },
  { value: "calendar", label: "日历", icon: CalendarDays },
  { value: "gantt", label: "甘特", icon: GanttIcon },
  { value: "spreadsheet", label: "表格", icon: Table },
] as const;

// Calendar view wrapper — bridges calendar-view's different prop interface
// using useStore + useIssues internally (lazy-loaded separately)
const CalendarViewWrapper = observer(function CalendarViewWrapper({
  workspaceId,
  projectId,
}: {
  workspaceId: string;
  projectId: string;
}) {
  const store = useStore();
  const { data: issues, isLoading } = useIssues(projectId, {
    state: store.issue.filters.stateIds,
    priority: store.issue.filters.priorityIds,
    assignee: store.issue.filters.assigneeIds,
    searchQuery: store.issue.filters.searchQuery,
  });

  const handleDragAndDrop = useCallback(
    async (
      _issueId: string | undefined,
      _issueProjectId: string | undefined,
      _sourceDate: string | undefined,
      _destinationDate: string | undefined
    ) => {
      // Phase 17: Calendar drag-and-drop is read-only; no server sync yet
    },
    []
  );

  return (
    <IssuesCalendarView
      issues={issues ?? []}
      isLoading={isLoading}
      projectId={projectId}
      workspaceId={workspaceId}
      handleDragAndDrop={handleDragAndDrop}
    />
  );
});

const IssuesPage = observer(function IssuesPage() {
  const { workspaceId, projectId } = useParams<{ workspaceId: string; projectId: string }>();
  const navigate = useNavigate();
  const store = useStore();
  const [createModalOpen, setCreateModalOpen] = useState(false);
  const activeView = store.issue.activeView;

  if (!workspaceId || !projectId) {
    return null;
  }

  const handleViewChange = useCallback(
    (view: string) => {
      store.issue.setActiveView(view as typeof activeView);
    },
    [store.issue]
  );

  return (
    <div className="flex h-full flex-col">
      {/* Page header — responsive padding */}
      <div className="flex items-center justify-between overflow-x-auto border-b border-custom-border-200 px-4 py-3 md:px-6">
        <div className="flex items-center gap-3">
          <h1 className="flex-shrink-0 text-lg font-semibold text-custom-text-100">Issues</h1>

          {/* View toggle — 5 buttons, responsive: hide labels on narrow screens */}
          <div className="flex items-center gap-0.5 rounded-md border border-custom-border-200 bg-custom-background-90 p-0.5">
            {VIEW_OPTIONS.map((option) => {
              const Icon = option.icon;
              const isActive = activeView === option.value;
              return (
                <button
                  key={option.value}
                  type="button"
                  onClick={() => handleViewChange(option.value)}
                  className={cn(
                    "flex items-center gap-1 rounded-sm px-2 py-1 text-xs transition-colors",
                    isActive
                      ? "bg-custom-background-100 text-custom-text-100 shadow-sm"
                      : "text-custom-text-400 hover:text-custom-text-200"
                  )}
                >
                  <Icon className="size-3.5" />
                  <span className="hidden md:inline">{option.label}</span>
                </button>
              );
            })}
          </div>
        </div>

        {/* Create Issue button */}
        <button
          type="button"
          onClick={() => setCreateModalOpen(true)}
          className="flex items-center gap-1.5 rounded-md bg-custom-primary px-3 py-2 text-sm font-medium text-white hover:bg-custom-primary/90 transition-colors"
        >
          <Plus className="size-4" />
          创建 Issue
        </button>
      </div>

      {/* Content area — 5 views with lazy loading */}
      <div className="flex-1 overflow-auto">
        {activeView === "list" && (
          <IssueListView workspaceId={workspaceId} projectId={projectId} />
        )}
        {activeView === "kanban" && (
          <Suspense fallback={<div className="flex items-center justify-center h-full text-custom-text-400">加载中...</div>}>
            <IssuesKanbanView workspaceId={workspaceId} projectId={projectId} />
          </Suspense>
        )}
        {activeView === "calendar" && (
          <Suspense fallback={<div className="flex items-center justify-center h-full text-custom-text-400">加载中...</div>}>
            <CalendarViewWrapper workspaceId={workspaceId} projectId={projectId} />
          </Suspense>
        )}
        {activeView === "gantt" && (
          <Suspense fallback={<div className="flex items-center justify-center h-full text-custom-text-400">加载中...</div>}>
            <IssuesGanttView workspaceId={workspaceId} projectId={projectId} />
          </Suspense>
        )}
        {activeView === "spreadsheet" && (
          <Suspense fallback={<div className="flex items-center justify-center h-full text-custom-text-400">加载中...</div>}>
            <IssuesSpreadsheetView workspaceId={workspaceId} projectId={projectId} />
          </Suspense>
        )}
      </div>

      {/* Create Issue modal */}
      <IssueCreateModal
        isOpen={createModalOpen}
        onClose={() => setCreateModalOpen(false)}
        workspaceId={workspaceId}
        projectId={projectId}
      />
    </div>
  );
});

export default IssuesPage;
