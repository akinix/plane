// FLOW: TanStack Query hooks for workspace data (mock data layer per D-P15-12)
import { useQuery } from "@tanstack/react-query";
import { useStore } from "@/lib/store-context";
import type { IWorkspace } from "@plane/types";
import { MOCK_WORKSPACES } from "../mock-data";

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export const useWorkspaces = () => {
  return useQuery<IWorkspace[]>({
    queryKey: ["workspaces"],
    queryFn: async () => {
      await delay(300);
      return MOCK_WORKSPACES;
    },
  });
};

export const useWorkspace = (workspaceId: string) => {
  return useQuery<IWorkspace | undefined>({
    queryKey: ["workspace", workspaceId],
    queryFn: async () => {
      await delay(200);
      return MOCK_WORKSPACES.find((ws) => ws.id === workspaceId);
    },
    enabled: !!workspaceId,
  });
};

export const useCurrentWorkspace = () => {
  const { workspace } = useStore();
  const currentWorkspaceId = workspace.currentWorkspaceId;

  return useQuery<IWorkspace | undefined>({
    queryKey: ["workspace", currentWorkspaceId],
    queryFn: async () => {
      await delay(200);
      if (!currentWorkspaceId) return undefined;
      return MOCK_WORKSPACES.find((ws) => ws.id === currentWorkspaceId);
    },
    enabled: !!currentWorkspaceId,
  });
};
