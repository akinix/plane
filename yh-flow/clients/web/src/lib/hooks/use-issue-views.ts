// FLOW: TanStack Query hooks for IssueView CRUD (per D-P17-14, FILT-04)
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type { TIssueView } from "@/components/issues/filters/types";
import { issueViewService } from "../services/issue-view.service";

/**
 * Fetch all views for a project
 */
export function useIssueViews(projectId: string) {
  return useQuery<TIssueView[]>({
    queryKey: ["issue-views", projectId],
    queryFn: async () => {
      return issueViewService.getIssueViews(projectId);
    },
    enabled: !!projectId,
  });
}

/**
 * Create a new issue view mutation
 */
export function useCreateIssueView() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: Omit<TIssueView, "id" | "createdAt" | "updatedAt">) => {
      return issueViewService.createIssueView(data);
    },
    onSuccess: (createdView) => {
      queryClient.invalidateQueries({ queryKey: ["issue-views", createdView.projectId] });
    },
  });
}
