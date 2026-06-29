// FLOW: TanStack Query hooks for Cycle (mock data layer per D-P18-04)
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type { ICycle, TProgressSnapshot } from "@plane/types";
import { MOCK_CYCLES } from "../mock-data";

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export const useCycles = (projectId: string) => {
  return useQuery<ICycle[]>({
    queryKey: ["cycles", projectId],
    queryFn: async () => {
      await delay(250);
      return MOCK_CYCLES.filter((c) => c.project_id === projectId);
    },
    enabled: !!projectId,
  });
};

export const useCycleDetail = (projectId: string, cycleId: string) => {
  return useQuery<ICycle | undefined>({
    queryKey: ["cycles", projectId, cycleId],
    queryFn: async () => {
      await delay(200);
      return MOCK_CYCLES.find((c) => c.id === cycleId && c.project_id === projectId);
    },
    enabled: !!projectId && !!cycleId,
  });
};

export const useCycleMutations = () => {
  const queryClient = useQueryClient();

  const createCycle = useMutation({
    mutationFn: async (data: Partial<ICycle>) => {
      await delay(300);
      const maxSort = MOCK_CYCLES.reduce((max, c) => Math.max(max, c.sort_order), 0);
      const newCycle: ICycle = {
        id: `cycle-mock-${Date.now()}`,
        name: data.name ?? "",
        description: data.description ?? "",
        start_date: data.start_date ?? null,
        end_date: data.end_date ?? null,
        status: data.status ?? "draft",
        sort_order: maxSort + 10000,
        project_id: data.project_id ?? "",
        workspace_id: data.workspace_id ?? "",
        owned_by_id: "user-1",
        created_by: "user-1",
        created_at: new Date().toISOString(),
        updated_at: new Date().toISOString(),
        updated_by: "user-1",
        archived_at: null,
        assignee_ids: [],
        view_props: { filters: {} },
        project_detail: { id: data.project_id ?? "" },
        progress: [],
        version: 1,

        // TProgressSnapshot fields
        total_issues: 0,
        completed_issues: 0,
        backlog_issues: 0,
        started_issues: 0,
        unstarted_issues: 0,
        cancelled_issues: 0,
        backlog_estimate_points: 0,
        started_estimate_points: 0,
        unstarted_estimate_points: 0,
        cancelled_estimate_points: 0,
        progress_snapshot: undefined,
        ...data,
      };
      MOCK_CYCLES.push(newCycle);
      return newCycle;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["cycles"] });
    },
  });

  const updateCycle = useMutation({
    mutationFn: async ({ cycleId, data }: { cycleId: string; data: Partial<ICycle> }) => {
      await delay(200);
      const idx = MOCK_CYCLES.findIndex((c) => c.id === cycleId);
      if (idx >= 0) {
        MOCK_CYCLES[idx] = { ...MOCK_CYCLES[idx], ...data, updated_at: new Date().toISOString() };
      }
      return MOCK_CYCLES[idx];
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["cycles"] });
    },
  });

  const deleteCycle = useMutation({
    mutationFn: async (cycleId: string) => {
      await delay(200);
      const idx = MOCK_CYCLES.findIndex((c) => c.id === cycleId);
      if (idx >= 0) {
        MOCK_CYCLES.splice(idx, 1);
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["cycles"] });
    },
  });

  return { createCycle, updateCycle, deleteCycle };
};

export const useCycleProgress = (projectId: string, cycleId: string) => {
  return useQuery<TProgressSnapshot | undefined>({
    queryKey: ["cycles", projectId, cycleId, "progress"],
    queryFn: async () => {
      await delay(200);
      const cycle = MOCK_CYCLES.find((c) => c.id === cycleId && c.project_id === projectId);
      return cycle?.progress_snapshot;
    },
    enabled: !!cycleId,
  });
};
