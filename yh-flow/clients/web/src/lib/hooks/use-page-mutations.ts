// FLOW: Forked from Plane — TanStack Query mutations for Page CRUD, archive, favorite (mock data layer per D-P19-05)
import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { TPage } from "@plane/types";
import { EPageAccess } from "@plane/types";
import { MOCK_PAGES } from "../mock-data";

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export const usePageMutations = () => {
  const queryClient = useQueryClient();

  const createPage = useMutation({
    mutationFn: async (data: Partial<TPage>) => {
      await delay(300);
      const newPage: TPage = {
        id: `page-mock-${Date.now()}`,
        name: data.name ?? "",
        description_html: data.description_html ?? "",
        description_json: data.description_json ?? {},
        access: data.access ?? EPageAccess.PUBLIC,
        is_favorite: data.is_favorite ?? false,
        is_locked: data.is_locked ?? false,
        archived_at: data.archived_at ?? null,
        owned_by: data.owned_by ?? "user-1",
        workspace: data.workspace ?? "",
        color: data.color ?? undefined,
        logo_props: data.logo_props ?? undefined,
        label_ids: data.label_ids ?? [],
        project_ids: data.project_ids ?? undefined,
        created_at: new Date(),
        updated_at: new Date(),
        created_by: data.created_by ?? "user-1",
        updated_by: data.updated_by ?? "user-1",
        deleted_at: undefined,
      };
      MOCK_PAGES.push(newPage);
      return newPage;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["pages"] });
    },
  });

  const updatePage = useMutation({
    mutationFn: async ({ pageId, data }: { pageId: string; data: Partial<TPage> }) => {
      await delay(200);
      const idx = MOCK_PAGES.findIndex((p) => p.id === pageId);
      if (idx >= 0) {
        MOCK_PAGES[idx] = { ...MOCK_PAGES[idx], ...data, updated_at: new Date() };
      }
      return MOCK_PAGES[idx];
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["pages"] });
    },
  });

  const deletePage = useMutation({
    mutationFn: async (pageId: string) => {
      await delay(200);
      const idx = MOCK_PAGES.findIndex((p) => p.id === pageId);
      if (idx >= 0) {
        MOCK_PAGES.splice(idx, 1);
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["pages"] });
    },
  });

  const archivePage = useMutation({
    mutationFn: async (pageId: string) => {
      await delay(200);
      const idx = MOCK_PAGES.findIndex((p) => p.id === pageId);
      if (idx >= 0) {
        MOCK_PAGES[idx].archived_at = new Date().toISOString();
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["pages"] });
    },
  });

  const restorePage = useMutation({
    mutationFn: async (pageId: string) => {
      await delay(200);
      const idx = MOCK_PAGES.findIndex((p) => p.id === pageId);
      if (idx >= 0) {
        MOCK_PAGES[idx].archived_at = null;
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["pages"] });
    },
  });

  const favoritePage = useMutation({
    mutationFn: async ({ pageId, is_favorite }: { pageId: string; is_favorite: boolean }) => {
      await delay(150);
      const idx = MOCK_PAGES.findIndex((p) => p.id === pageId);
      if (idx >= 0) {
        MOCK_PAGES[idx].is_favorite = is_favorite;
      }
      return MOCK_PAGES[idx];
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["pages"] });
    },
  });

  return { createPage, updatePage, deletePage, archivePage, restorePage, favoritePage };
};
