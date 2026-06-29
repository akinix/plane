// FLOW: TanStack Query hooks for issue data (mock data layer per D-P16-02)
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type { TIssue } from "@plane/types";
import { MOCK_ISSUES, MOCK_STATES } from "../mock-data";

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export const useIssues = (projectId: string) => {
  return useQuery<TIssue[]>({
    queryKey: ["issues", projectId],
    queryFn: async () => {
      await delay(300);
      return MOCK_ISSUES.filter((i) => i.project_id === projectId);
    },
    enabled: !!projectId,
  });
};

export const useIssue = (projectId: string, issueId: string) => {
  return useQuery<TIssue | undefined>({
    queryKey: ["issues", projectId, issueId],
    queryFn: async () => {
      await delay(200);
      return MOCK_ISSUES.find((i) => i.id === issueId && i.project_id === projectId);
    },
    enabled: !!projectId && !!issueId,
  });
};

export const useProjectStates = (projectId: string) => {
  return useQuery({
    queryKey: ["states", projectId],
    queryFn: async () => {
      await delay(150);
      return MOCK_STATES.filter((s) => s.project_id === projectId);
    },
    enabled: !!projectId,
  });
};

export const useIssueMutations = () => {
  const queryClient = useQueryClient();

  const updateIssue = useMutation({
    mutationFn: async ({ issueId, data }: { issueId: string; data: Partial<TIssue> }) => {
      await delay(200);
      const index = MOCK_ISSUES.findIndex((i) => i.id === issueId);
      if (index >= 0) {
        MOCK_ISSUES[index] = { ...MOCK_ISSUES[index], ...data, updated_at: new Date().toISOString() };
      }
      return { issueId, ...data };
    },
    onMutate: async ({ issueId, data }) => {
      await queryClient.cancelQueries({ queryKey: ["issues"] });
      const previousQueries = queryClient.getQueriesData<TIssue[]>({ queryKey: ["issues"] });
      queryClient.setQueriesData<TIssue[]>({ queryKey: ["issues"] }, (old) =>
        old?.map((i) => (i.id === issueId ? { ...i, ...data, updated_at: new Date().toISOString() } : i))
      );
      return { previousQueries };
    },
    onError: (_err, _vars, context) => {
      if (context?.previousQueries) {
        for (const [key, data] of context.previousQueries) {
          queryClient.setQueryData(key, data);
        }
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ["issues"] });
    },
  });

  const deleteIssue = useMutation({
    mutationFn: async ({ issueId }: { issueId: string }) => {
      await delay(300);
      const index = MOCK_ISSUES.findIndex((i) => i.id === issueId);
      if (index >= 0) {
        MOCK_ISSUES[index] = {
          ...MOCK_ISSUES[index],
          archived_at: new Date().toISOString(),
          updated_at: new Date().toISOString(),
        };
      }
      return { issueId };
    },
    onMutate: async ({ issueId }) => {
      await queryClient.cancelQueries({ queryKey: ["issues"] });
      const previous = queryClient.getQueriesData<TIssue[]>({ queryKey: ["issues"] });
      queryClient.setQueriesData<TIssue[]>({ queryKey: ["issues"] }, (old) =>
        old?.map((i) =>
          i.id === issueId ? { ...i, archived_at: new Date().toISOString(), updated_at: new Date().toISOString() } : i
        )
      );
      return { previous };
    },
    onError: (_err, _vars, context) => {
      if (context?.previous) {
        for (const [key, data] of context.previous) {
          queryClient.setQueryData(key, data);
        }
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ["issues"] });
    },
  });

  return { updateIssue, deleteIssue };
};
