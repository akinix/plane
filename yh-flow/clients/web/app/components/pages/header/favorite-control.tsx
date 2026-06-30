// FLOW: Forked from Plane. Original: apps/web/core/components/pages/header/favorite-control.tsx
// FLOW: PageFavoriteControl — 页面收藏星标控制
"use client";
import { Star } from "lucide-react";
import type { TPage } from "@plane/types";
import { usePageMutations } from "@/../src/lib/hooks/use-page-mutations";
import { cn } from "@plane/utils";
import { useQueryClient } from "@tanstack/react-query";

type Props = {
  page: TPage;
};

export const PageFavoriteControl = function PageFavoriteControl({ page }: Props) {
  const queryClient = useQueryClient();
  const { favoritePage } = usePageMutations();
  const { id, is_favorite } = page;

  const handleToggleFavorite = (e: React.MouseEvent) => {
    e.stopPropagation();
    e.preventDefault();
    if (id) {
      favoritePage.mutate({ pageId: id, is_favorite: !is_favorite }, {
        onSettled: () => {
          queryClient.invalidateQueries({ queryKey: ["pages"] });
        },
      });
    }
  };

  return (
    <button
      type="button"
      onClick={handleToggleFavorite}
      className={cn("grid size-7 place-items-center rounded transition-colors", "hover:bg-custom-background-80")}
      aria-label={is_favorite ? "取消收藏" : "收藏"}
    >
      <Star
        className={cn(
          "size-4 transition-colors",
          is_favorite ? "fill-yellow-500 text-yellow-500" : "text-custom-text-400"
        )}
      />
    </button>
  );
};
