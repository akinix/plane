// FLOW: Forked from Plane — TanStack Query hooks for Project Views (per D-P19-09)
// Shares queryKey ["issue-views"] with useIssueViews from use-issue-views.ts
import { useQuery } from "@tanstack/react-query";
import type { TIssueView } from "@/components/issues/filters/types";
import { issueViewService } from "../services/issue-view.service";

/**
 * Fetch all views for a project
 */
export const useViews = (projectId: string) => {
  return useQuery<TIssueView[]>({
    queryKey: ["issue-views", projectId],
    queryFn: async () => {
      return issueViewService.getIssueViews(projectId);
    },
    enabled: !!projectId,
  });
};

/**
 * Fetch a single view detail
 */
export const useViewDetail = (projectId: string, viewId: string) => {
  return useQuery<TIssueView | undefined>({
    queryKey: ["issue-views", projectId, viewId],
    queryFn: async () => {
      return issueViewService.getIssueView(viewId);
    },
    enabled: !!projectId && !!viewId,
  });
};
