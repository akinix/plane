// FLOW: Forked from Plane — TanStack Query hooks for Page (mock data layer per D-P19-05)
import { useQuery } from "@tanstack/react-query";
import type { TPage } from "@plane/types";
import { MOCK_PAGES } from "../mock-data";

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export const usePages = (workspaceId: string) => {
  return useQuery<TPage[]>({
    queryKey: ["pages", workspaceId],
    queryFn: async () => {
      await delay(250);
      return MOCK_PAGES.filter((p) => p.workspace === workspaceId && !p.archived_at);
    },
    enabled: !!workspaceId,
  });
};

export const usePageDetail = (workspaceId: string, pageId: string) => {
  return useQuery<TPage | undefined>({
    queryKey: ["pages", workspaceId, pageId],
    queryFn: async () => {
      await delay(200);
      return MOCK_PAGES.find((p) => p.id === pageId && p.workspace === workspaceId);
    },
    enabled: !!workspaceId && !!pageId,
  });
};

export const useArchivedPages = (workspaceId: string) => {
  return useQuery<TPage[]>({
    queryKey: ["pages", "archived", workspaceId],
    queryFn: async () => {
      await delay(250);
      return MOCK_PAGES.filter((p) => p.workspace === workspaceId && p.archived_at);
    },
    enabled: !!workspaceId,
  });
};
