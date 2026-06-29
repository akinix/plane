// FLOW: Forked from Plane calendar/roots/base-calendar-root.tsx
// Adapted: replaced @plane/hooks/stores with yh-flow useIssues (TanStack Query) + useIssueMutations
// Removed Next.js router, Plane permission/issue store types
import { useCallback } from "react";
import { useIssues, useIssueMutations } from "@/../src/lib/hooks/use-issues";
import { useStore } from "@/lib/store-context";
import { CalendarView } from "../calendar-view";

type TProps = {
  workspaceId: string;
  projectId: string;
};

export const BaseCalendarRoot = function BaseCalendarRoot({ workspaceId, projectId }: TProps) {
  const store = useStore();
  const { updateIssue } = useIssueMutations();
  const { data: issues, isLoading } = useIssues(projectId, {
    state: store.issue.filters.stateIds.length > 0 ? store.issue.filters.stateIds : undefined,
    priority: store.issue.filters.priorityIds.length > 0 ? store.issue.filters.priorityIds : undefined,
    assignee: store.issue.filters.assigneeIds.length > 0 ? store.issue.filters.assigneeIds : undefined,
    searchQuery: store.issue.filters.searchQuery || undefined,
  });

  const handleDragAndDrop = useCallback(
    async (
      issueId: string | undefined,
      _issueProjectId: string | undefined,
      _sourceDate: string | undefined,
      destinationDate: string | undefined
    ) => {
      if (!issueId || !destinationDate) return;
      updateIssue.mutate({ issueId, data: { target_date: destinationDate } });
    },
    [updateIssue]
  );

  return (
    <CalendarView
      issues={issues ?? []}
      isLoading={isLoading}
      projectId={projectId}
      workspaceId={workspaceId}
      handleDragAndDrop={handleDragAndDrop}
    />
  );
};
