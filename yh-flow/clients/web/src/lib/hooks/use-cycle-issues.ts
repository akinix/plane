// FLOW: TanStack Query hooks for Cycle-Issue associations (mock data layer per D-P18-04)
import { useMemo } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type { TIssue } from "@plane/types";
import { MOCK_ISSUES, MOCK_STATES } from "../mock-data";

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

export const useCycleProgress = (projectId: string, cycleId: string) => {
  const { data: cycleIssues, isLoading } = useCycleIssues(projectId, cycleId);

  const progress = useMemo(() => {
    const getStateGroup = (issue: TIssue): string => {
      const state = MOCK_STATES.find((s: { id: string; group: string }) => s.id === issue.state_id);
      return state?.group ?? "backlog";
    };
    const completed = cycleIssues?.filter((i) => getStateGroup(i) === "completed").length ?? 0;
    const total = cycleIssues?.length ?? 0;
    return {
      total_issues: total,
      completed_issues: completed,
      backlog_issues: cycleIssues?.filter((i) => getStateGroup(i) === "backlog").length ?? 0,
      unstarted_issues: cycleIssues?.filter((i) => getStateGroup(i) === "unstarted").length ?? 0,
      started_issues: cycleIssues?.filter((i) => getStateGroup(i) === "started").length ?? 0,
      cancelled_issues: cycleIssues?.filter((i) => getStateGroup(i) === "cancelled").length ?? 0,
      start_date: "",
      end_date: "",
      burndown: [] as { date: string; ideal: number; actual: number }[],
    };
  }, [cycleIssues]);

  return { data: progress, isLoading, isError: false };
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
