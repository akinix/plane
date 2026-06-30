// FLOW: Forked from Plane — TanStack Query mutations for View CRUD + favorite (per D-P19-09)
import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { TIssueView } from "@/components/issues/filters/types";
import { issueViewService } from "../services/issue-view.service";

/**
 * View mutations: create, update, delete, favorite
 */
export const useViewMutations = () => {
  const queryClient = useQueryClient();

  const createView = useMutation({
    mutationFn: async (data: Omit<TIssueView, "id" | "createdAt" | "updatedAt">) => {
      return issueViewService.createIssueView(data);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["issue-views"] });
    },
  });

  const updateView = useMutation({
    mutationFn: async ({ viewId, data }: { viewId: string; data: Partial<TIssueView> }) => {
      return issueViewService.updateIssueView(viewId, data);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["issue-views"] });
    },
  });

  const deleteView = useMutation({
    mutationFn: async (viewId: string) => {
      return issueViewService.deleteIssueView(viewId);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["issue-views"] });
    },
  });

  const favoriteView = useMutation({
    mutationFn: async ({ viewId, isFavorite }: { viewId: string; isFavorite: boolean }) => {
      return issueViewService.favoriteIssueView(viewId, isFavorite);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["issue-views"] });
    },
  });

  return { createView, updateView, deleteView, favoriteView };
};
