// FLOW: Forked from Plane. Original: apps/web/core/components/pages/list/block.tsx
// FLOW: PageListBlock — 单个页面卡片（Logo + 名称 + 权限徽章 + 收藏星标 + 日期）
"use client";
import { useNavigate } from "react-router";
import { Star, Earth, Lock, FileText } from "lucide-react";
import type { TPage } from "@plane/types";
import { EPageAccess } from "@plane/types";
import { usePageMutations } from "@/../src/lib/hooks/use-page-mutations";
import { cn, renderFormattedDate } from "@plane/utils";
import { useQueryClient } from "@tanstack/react-query";

type Props = {
  page: TPage;
  workspaceId: string;
};

export const PageListBlock = function PageListBlock({ page, workspaceId }: Props) {
  const navigate = useNavigate();
  const { favoritePage } = usePageMutations();
  const queryClient = useQueryClient();

  const { id, name, logo_props, access, is_favorite, updated_at, archived_at } = page;

  const handleClick = () => {
    navigate(`/workspaces/${workspaceId}/pages/${id}`);
  };

  const handleFavorite = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (id) {
      favoritePage.mutate({ pageId: id, is_favorite: !is_favorite }, {
        onError: () => {
          queryClient.invalidateQueries({ queryKey: ["pages"] });
        },
      });
    }
  };

  // Determine icon to display
  const renderIcon = () => {
    if (logo_props?.in_use === "emoji" && logo_props?.emoji?.value) {
      const code = parseInt(logo_props.emoji.value, 10);
      if (Number.isSafeInteger(code) && code > 0) {
        return <span className="text-lg">{String.fromCodePoint(code)}</span>;
      }
    }
    return <FileText className="text-custom-text-300 size-5" />;
  };

  return (
    <button
      type="button"
      onClick={handleClick}
      className={cn(
        "group border-custom-border-200 bg-custom-background-90 flex flex-col gap-3 rounded-lg border p-4 text-left",
        "hover:border-custom-border-100 hover:shadow-sm transition-all",
        "cursor-pointer"
      )}
    >
      {/* Top row: icon + name + favorite */}
      <div className="flex items-start gap-3">
        {/* Icon */}
        <div className="bg-custom-background-80 flex size-10 items-center justify-center rounded-md">
          {renderIcon()}
        </div>

        {/* Name and badge */}
        <div className="min-w-0 flex-1">
          <div className="flex items-center gap-2">
            <span className="text-sm text-custom-text-100 truncate font-medium">{name ?? "未命名页面"}</span>
            {archived_at && (
              <span className="bg-custom-background-80 text-custom-text-400 flex-shrink-0 rounded px-1.5 py-0.5 text-[10px]">
                已归档
              </span>
            )}
          </div>
          <div className="mt-0.5 flex items-center gap-1.5">
            {/* Access badge */}
            <span className="text-custom-text-300 flex items-center gap-1 text-[11px]">
              {access === EPageAccess.PUBLIC ? (
                <>
                  <Earth className="size-3" />
                  公开
                </>
              ) : (
                <>
                  <Lock className="size-3" />
                  私人
                </>
              )}
            </span>
          </div>
        </div>

        {/* Favorite star */}
        <button onClick={handleFavorite} className="flex-shrink-0 p-0.5 transition-opacity hover:opacity-80">
          <Star
            className={cn(
              "size-4 transition-colors",
              is_favorite ? "fill-yellow-500 text-yellow-500" : "text-custom-text-400 opacity-0 group-hover:opacity-100"
            )}
          />
        </button>
      </div>

      {/* Bottom row: date */}
      <div className="text-xs text-custom-text-400">{updated_at && `更新于 ${renderFormattedDate(updated_at)}`}</div>
    </button>
  );
};
