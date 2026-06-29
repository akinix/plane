// FLOW: TanStack Query hooks for Cycle-Issue associations (mock data layer per D-P18-04)
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type { TIssue } from "@plane/types";
import { MOCK_ISSUES } from "../mock-data";

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export const useCycleIssues = (projectId: string, cycleId: string) => {
  return useQuery<TIssue[]>({
    queryKey: ["cycles", projectId, cycleId, "issues"],
    queryFn: async () => {
      await delay(200);
      return MOCK_ISSUES.filter((i) => i.project_id === projectId && i.cycle_id === cycleId);
    },
    enabled: !!projectId && !!cycleId,
  });
};

export const useAddIssueToCycle = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ issueId, cycleId }: { issueId: string; cycleId: string }) => {
      await delay(200);
      const issue = MOCK_ISSUES.find((i) => i.id === issueId);
      if (issue) {
        issue.cycle_id = cycleId;
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["cycles"] });
      queryClient.invalidateQueries({ queryKey: ["issues"] });
    },
  });
};

export const useRemoveIssueFromCycle = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (issueId: string) => {
      await delay(200);
      const issue = MOCK_ISSUES.find((i) => i.id === issueId);
      if (issue) {
        issue.cycle_id = null;
      }
    },
    onMutate: async (issueId: string) => {
      await queryClient.cancelQueries({ queryKey: ["cycles"] });
      await queryClient.cancelQueries({ queryKey: ["issues"] });
      const previousCyclesData = queryClient.getQueriesData({ queryKey: ["cycles"] });
      const previousIssuesData = queryClient.getQueriesData({ queryKey: ["issues"] });

      // Optimistically update issues cache
      queryClient.setQueriesData<TIssue[]>({ queryKey: ["issues"] }, (old) =>
        old?.map((i) => (i.id === issueId ? { ...i, cycle_id: null } : i))
      );

      return { previousCyclesData, previousIssuesData };
    },
    onError: (_err, _vars, context) => {
      if (context?.previousIssuesData) {
        for (const [key, data] of context.previousIssuesData) {
          queryClient.setQueryData(key, data);
        }
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ["cycles"] });
      queryClient.invalidateQueries({ queryKey: ["issues"] });
    },
  });
};

export const useTransferCycleIssues = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ fromCycleId, toCycleId }: { fromCycleId: string; toCycleId: string }) => {
      await delay(300);
      for (const issue of MOCK_ISSUES) {
        if (issue.cycle_id === fromCycleId) {
          issue.cycle_id = toCycleId;
        }
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["cycles"] });
      queryClient.invalidateQueries({ queryKey: ["issues"] });
    },
  });
};
