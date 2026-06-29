// FLOW: TanStack Query hooks for Issue CRUD (mock data layer per D-P16-02)
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type { TIssue, TBulkOperationsPayload } from "@plane/types";
import { MOCK_ISSUES } from "../mock-data";

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export type IssueFilters = {
  state?: string[];
  priority?: string[];
  assignee?: string[];
  searchQuery?: string;
};

export const useIssues = (projectId: string, filters?: IssueFilters) => {
  return useQuery<TIssue[]>({
    queryKey: ["issues", projectId, filters],
    queryFn: async () => {
      await delay(300);
      let result = MOCK_ISSUES.filter((i) => i.project_id === projectId);

      if (filters?.state && filters.state.length > 0) {
        result = result.filter((i) => i.state_id && filters.state!.includes(i.state_id));
      }
      if (filters?.priority && filters.priority.length > 0) {
        result = result.filter((i) => i.priority && filters.priority!.includes(i.priority));
      }
      if (filters?.assignee && filters.assignee.length > 0) {
        result = result.filter((i) => i.assignee_ids.some((aid) => filters.assignee!.includes(aid)));
      }
      if (filters?.searchQuery) {
        const q = filters.searchQuery.toLowerCase();
        result = result.filter((i) => i.name.toLowerCase().includes(q));
      }

      return result;
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

export const useIssueMutations = () => {
  const queryClient = useQueryClient();

  const createIssue = useMutation({
    mutationFn: async (data: Partial<TIssue>) => {
      await delay(300);
      const maxSeq = MOCK_ISSUES.filter((i) => i.project_id === data.project_id).reduce(
        (max, i) => Math.max(max, i.sequence_id),
        0
      );
      const maxSort = MOCK_ISSUES.reduce((max, i) => Math.max(max, i.sort_order), 0);
      const newIssue: TIssue = {
        id: `issue-mock-${Date.now()}`,
        sequence_id: maxSeq + 1,
        sort_order: maxSort + 10000,
        name: data.name ?? "",
        project_id: data.project_id ?? null,
        state_id: data.state_id ?? null,
        priority: data.priority ?? null,
        assignee_ids: data.assignee_ids ?? [],
        label_ids: data.label_ids ?? [],
        estimate_point: data.estimate_point ?? null,
        sub_issues_count: 0,
        attachment_count: 0,
        link_count: 0,
        parent_id: null,
        cycle_id: null,
        module_ids: null,
        type_id: null,
        is_draft: false,
        created_at: new Date().toISOString(),
        updated_at: new Date().toISOString(),
        start_date: null,
        target_date: null,
        completed_at: null,
        archived_at: null,
        created_by: "user-1",
        updated_by: "user-1",
        ...data,
      };
      MOCK_ISSUES.push(newIssue);
      return newIssue;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["issues"] });
    },
  });

  const updateIssue = useMutation({
    mutationFn: async ({ issueId, data }: { issueId: string; data: Partial<TIssue> }) => {
      await delay(200);
      const idx = MOCK_ISSUES.findIndex((i) => i.id === issueId);
      if (idx >= 0) {
        MOCK_ISSUES[idx] = { ...MOCK_ISSUES[idx], ...data, updated_at: new Date().toISOString() };
      }
      return MOCK_ISSUES[idx];
    },
    onMutate: async ({ issueId, data }) => {
      await queryClient.cancelQueries({ queryKey: ["issues"] });
      const previousData = queryClient.getQueryData(["issues"]);
      queryClient.setQueriesData({ queryKey: ["issues"] }, (old: unknown) => {
        if (Array.isArray(old)) {
          return old.map((i: TIssue) =>
            i.id === issueId ? { ...i, ...data, updated_at: new Date().toISOString() } : i
          );
        }
        return old;
      });
      return { previousData };
    },
    onError: (_err, _vars, context) => {
      if (context?.previousData) {
        queryClient.setQueriesData({ queryKey: ["issues"] }, context.previousData);
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ["issues"] });
    },
  });

  const deleteIssue = useMutation({
    mutationFn: async (issueId: string) => {
      await delay(200);
      const idx = MOCK_ISSUES.findIndex((i) => i.id === issueId);
      if (idx >= 0) {
        MOCK_ISSUES[idx] = {
          ...MOCK_ISSUES[idx],
          archived_at: new Date().toISOString(),
          updated_at: new Date().toISOString(),
        };
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["issues"] });
    },
  });

  const bulkUpdateIssues = useMutation({
    mutationFn: async (payload: TBulkOperationsPayload) => {
      await delay(300);
      for (const issueId of payload.issue_ids) {
        const idx = MOCK_ISSUES.findIndex((i) => i.id === issueId);
        if (idx >= 0) {
          MOCK_ISSUES[idx] = {
            ...MOCK_ISSUES[idx],
            ...payload.properties,
            updated_at: new Date().toISOString(),
          };
        }
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["issues"] });
    },
  });

  return { createIssue, updateIssue, deleteIssue, bulkUpdateIssues };
};
