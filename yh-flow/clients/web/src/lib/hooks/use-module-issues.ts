// FLOW: TanStack Query hooks for Module-Issue associations (mock data layer per D-P18-05)
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type { TIssue } from "@plane/types";
import { MOCK_ISSUES } from "../mock-data";

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export const useModuleIssues = (projectId: string, moduleId: string) => {
  return useQuery<TIssue[]>({
    queryKey: ["modules", projectId, moduleId, "issues"],
    queryFn: async () => {
      await delay(200);
      return MOCK_ISSUES.filter((i) => i.project_id === projectId && i.module_ids?.includes(moduleId));
    },
    enabled: !!projectId && !!moduleId,
  });
};

export const useAddIssueToModule = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ issueId, moduleId }: { issueId: string; moduleId: string }) => {
      await delay(200);
      const issue = MOCK_ISSUES.find((i) => i.id === issueId);
      if (issue) {
        if (!issue.module_ids) {
          issue.module_ids = [];
        }
        if (!issue.module_ids.includes(moduleId)) {
          issue.module_ids.push(moduleId);
        }
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["modules"] });
      queryClient.invalidateQueries({ queryKey: ["issues"] });
    },
  });
};

export const useRemoveIssueFromModule = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ issueId, moduleId }: { issueId: string; moduleId: string }) => {
      await delay(200);
      const issue = MOCK_ISSUES.find((i) => i.id === issueId);
      if (issue?.module_ids) {
        const idx = issue.module_ids.indexOf(moduleId);
        if (idx >= 0) {
          issue.module_ids.splice(idx, 1);
        }
      }
    },
    onMutate: async ({ issueId, moduleId }) => {
      await queryClient.cancelQueries({ queryKey: ["modules"] });
      await queryClient.cancelQueries({ queryKey: ["issues"] });
      const previousModulesData = queryClient.getQueriesData({ queryKey: ["modules"] });
      const previousIssuesData = queryClient.getQueriesData({ queryKey: ["issues"] });

      // Optimistically update issues cache: remove moduleId from module_ids
      queryClient.setQueriesData<TIssue[]>({ queryKey: ["issues"] }, (old) =>
        old?.map((i) =>
          i.id === issueId && i.module_ids
            ? { ...i, module_ids: i.module_ids.filter((m: string) => m !== moduleId) }
            : i
        )
      );

      return { previousModulesData, previousIssuesData };
    },
    onError: (_err, _vars, context) => {
      if (context?.previousIssuesData) {
        for (const [key, data] of context.previousIssuesData) {
          queryClient.setQueryData(key, data);
        }
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ["modules"] });
      queryClient.invalidateQueries({ queryKey: ["issues"] });
    },
  });
};
