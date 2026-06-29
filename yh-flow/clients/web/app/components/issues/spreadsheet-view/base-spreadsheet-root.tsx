// FLOW: Forked from Plane spreadsheet/base-spreadsheet-root.tsx
// Adapted for yh-flow — simplified root container without QuickActions / issue layout HOC
import { observer } from "mobx-react";
import { useStore } from "@/lib/store-context";
import { useIssues } from "@/../src/lib/hooks/use-issues";
import { SpreadsheetView } from "./spreadsheet-view";

type TProps = {
  workspaceId: string;
  projectId: string;
};

export const BaseSpreadsheetRoot = observer(function BaseSpreadsheetRoot({ workspaceId, projectId }: TProps) {
  const store = useStore();
  const { data: issues, isLoading } = useIssues(projectId, {
    state: store.issue.filters.stateIds,
    priority: store.issue.filters.priorityIds,
    assignee: store.issue.filters.assigneeIds,
    searchQuery: store.issue.filters.searchQuery,
  });

  const issueIds = issues?.map((i) => i.id) ?? [];

  if (isLoading) {
    return (
      <div className="flex flex-col gap-3 p-4">
        {Array.from({ length: 8 }).map((_, i) => (
          <div key={i} className="flex h-11 items-center gap-3 border-b border-custom-border-200 px-4">
            <div className="h-3 w-3 rounded bg-custom-background-80" />
            <div className="h-3 w-20 rounded bg-custom-background-80" />
            <div className="h-3 flex-1 rounded bg-custom-background-80" />
            <div className="h-3 w-16 rounded bg-custom-background-80" />
            <div className="h-6 w-6 rounded-full bg-custom-background-80" />
            <div className="h-3 w-24 rounded bg-custom-background-80" />
          </div>
        ))}
      </div>
    );
  }

  if (!issues || issues.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16">
        <div className="mb-3 rounded-full bg-custom-background-80 p-4">
          <svg className="size-8 text-custom-text-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
        </div>
        <h3 className="text-sm font-medium text-custom-text-200">暂无 Issue</h3>
        <p className="mt-1 text-xs text-custom-text-400">这个项目还没有创建 Issue。</p>
      </div>
    );
  }

  return (
    <SpreadsheetView
      workspaceId={workspaceId}
      projectId={projectId}
      issueIds={issueIds}
      issues={issues}
    />
  );
});
