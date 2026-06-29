// FLOW: TanStack Query hooks for Module (mock data layer per D-P18-05)
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type { IModule, ILinkDetails } from "@plane/types";
import { MOCK_MODULES } from "../mock-data";

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export const useModules = (projectId: string) => {
  return useQuery<IModule[]>({
    queryKey: ["modules", projectId],
    queryFn: async () => {
      await delay(250);
      return MOCK_MODULES.filter((m) => m.project_id === projectId);
    },
    enabled: !!projectId,
  });
};

export const useModuleDetail = (projectId: string, moduleId: string) => {
  return useQuery<IModule | undefined>({
    queryKey: ["modules", projectId, moduleId],
    queryFn: async () => {
      await delay(200);
      return MOCK_MODULES.find((m) => m.id === moduleId && m.project_id === projectId);
    },
    enabled: !!projectId && !!moduleId,
  });
};

export const useModuleMutations = () => {
  const queryClient = useQueryClient();

  const createModule = useMutation({
    mutationFn: async (data: Partial<IModule>) => {
      await delay(300);
      const maxSort = MOCK_MODULES.reduce((max, m) => Math.max(max, m.sort_order), 0);
      const newModule: IModule = {
        id: `module-mock-${Date.now()}`,
        name: data.name ?? "",
        description: data.description ?? "",
        description_text: null,
        description_html: null,
        workspace_id: data.workspace_id ?? "",
        project_id: data.project_id ?? "",
        lead_id: data.lead_id ?? null,
        member_ids: data.member_ids ?? [],
        is_favorite: false,
        sort_order: maxSort + 10000,
        view_props: { filters: {} },
        status: data.status ?? "planned",
        archived_at: null,
        start_date: data.start_date ?? null,
        target_date: data.target_date ?? null,
        created_at: new Date().toISOString(),
        updated_at: new Date().toISOString(),
        created_by: "user-1",
        updated_by: "user-1",

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
      };
      MOCK_MODULES.push(newModule);
      return newModule;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["modules"] });
    },
  });

  const updateModule = useMutation({
    mutationFn: async ({ moduleId, data }: { moduleId: string; data: Partial<IModule> }) => {
      await delay(200);
      const idx = MOCK_MODULES.findIndex((m) => m.id === moduleId);
      if (idx >= 0) {
        MOCK_MODULES[idx] = { ...MOCK_MODULES[idx], ...data, updated_at: new Date().toISOString() };
      }
      return MOCK_MODULES[idx];
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["modules"] });
    },
  });

  const deleteModule = useMutation({
    mutationFn: async (moduleId: string) => {
      await delay(200);
      const idx = MOCK_MODULES.findIndex((m) => m.id === moduleId);
      if (idx >= 0) {
        MOCK_MODULES.splice(idx, 1);
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["modules"] });
    },
  });

  return { createModule, updateModule, deleteModule };
};

export const useModuleLinkMutations = (moduleId: string) => {
  const queryClient = useQueryClient();

  const addLink = useMutation({
    mutationFn: async (link: ILinkDetails) => {
      await delay(200);
      const mod = MOCK_MODULES.find((m) => m.id === moduleId);
      if (mod) {
        if (!mod.link_module) mod.link_module = [];
        mod.link_module.push(link);
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["modules"] });
    },
  });

  const updateLink = useMutation({
    mutationFn: async ({ linkId, data }: { linkId: string; data: Partial<ILinkDetails> }) => {
      await delay(200);
      const mod = MOCK_MODULES.find((m) => m.id === moduleId);
      if (mod?.link_module) {
        const idx = mod.link_module.findIndex((l) => l.id === linkId);
        if (idx >= 0) {
          mod.link_module[idx] = { ...mod.link_module[idx], ...data };
        }
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["modules"] });
    },
  });

  const removeLink = useMutation({
    mutationFn: async (linkId: string) => {
      await delay(200);
      const mod = MOCK_MODULES.find((m) => m.id === moduleId);
      if (mod?.link_module) {
        const idx = mod.link_module.findIndex((l) => l.id === linkId);
        if (idx >= 0) {
          mod.link_module.splice(idx, 1);
        }
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["modules"] });
    },
  });

  return { addLink, updateLink, removeLink };
};
