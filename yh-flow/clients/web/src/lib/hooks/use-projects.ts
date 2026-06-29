// FLOW: TanStack Query hooks for project data (mock data layer per D-P15-12)
import { useQuery } from "@tanstack/react-query";
import type { IProject } from "@plane/types";
import { MOCK_PROJECTS } from "../mock-data";

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export const useProjects = (workspaceId: string) => {
  return useQuery<IProject[]>({
    queryKey: ["projects", workspaceId],
    queryFn: async () => {
      await delay(300);
      return MOCK_PROJECTS.filter((p) => p.workspace === workspaceId);
    },
    enabled: !!workspaceId,
  });
};

export const useProject = (workspaceId: string, projectId: string) => {
  return useQuery<IProject | undefined>({
    queryKey: ["projects", workspaceId, projectId],
    queryFn: async () => {
      await delay(200);
      return MOCK_PROJECTS.find(
        (p) => p.id === projectId && p.workspace === workspaceId
      );
    },
    enabled: !!workspaceId && !!projectId,
  });
};
