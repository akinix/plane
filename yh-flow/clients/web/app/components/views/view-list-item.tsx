// FLOW: Forked from Plane. Original: apps/web/core/components/views/view-list-item.tsx
// FLOW: ViewListItem — 视图卡片组件（名称、描述、权限徽章、收藏星标、创建人头像）
"use client";
import { useRef } from "react";
import { observer } from "mobx-react";
import { useNavigate } from "react-router";
import { Earth, Lock, Star } from "lucide-react";
import { useStore } from "@/lib/store-context";
import { useViewMutations } from "@/../src/lib/hooks/use-view-mutations";
import type { TIssueView } from "@/components/issues/filters/types";

// Extended type for view items with additional fields from mock data
interface TIssueViewExtended extends TIssueView {
  access: number;
  is_favorite: boolean;
  description: string;
  owned_by: string;
  created_by: string;
}
import { ViewListItemAction } from "./view-list-item-action";

type Props = {
  view: TIssueView;
  workspaceId: string;
  projectId: string;
};

const ViewListItem = observer(function ViewListItem({ view, workspaceId, projectId }: Props) {
  const navigate = useNavigate();
  const { view: viewStore } = useStore();
  const { favoriteView } = useViewMutations();
  const parentRef = useRef<HTMLDivElement>(null);
  const ext = view as TIssueViewExtended;
  const access: number = ext.access ?? 0;
  const isFav: boolean = ext.is_favorite ?? false;
  const description: string = ext.description ?? "";
  const ownedBy: string = ext.owned_by ?? "";

  const handleApply = () => {
    viewStore.setAppliedViewId(view.id);
    navigate(`/workspaces/${workspaceId}/projects/${projectId}/issues?view=${view.id}`);
  };

  const handleToggleFavorite = () => {
    favoriteView.mutate({ viewId: view.id, isFavorite: !isFav });
  };

  return (
    <div
      ref={parentRef}
      className="hover:bg-custom-background-80 flex items-center gap-3 px-4 py-3 transition-colors"
    >
      {/* View icon */}
      <div className="flex h-8 w-8 flex-shrink-0 items-center justify-center rounded bg-custom-background-80">
        <span className="text-sm">{(view.name ?? "")[0]}</span>
      </div>

      {/* View info */}
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2">
          <span className="truncate text-sm font-medium text-custom-text-100">{view.name}</span>
          {/* Access badge */}
          {access === 1 ? (
            <span className="flex items-center gap-1 rounded-sm bg-custom-background-80 px-1.5 py-0.5 text-2xs text-custom-text-300">
              <Lock className="size-3" />
              私人
            </span>
          ) : (
            <span className="flex items-center gap-1 rounded-sm bg-custom-background-80 px-1.5 py-0.5 text-2xs text-custom-text-300">
              <Earth className="size-3" />
              公开
            </span>
          )}
        </div>
        {description && (
          <p className="mt-0.5 truncate text-xs text-custom-text-300">
            {description}
          </p>
        )}
      </div>

      {/* Creator avatar */}
      <div className="flex flex-shrink-0 items-center">
        <div className="flex h-6 w-6 items-center justify-center rounded-full bg-custom-background-80 text-2xs text-custom-text-300">
          {ownedBy ? ownedBy.slice(-1).toUpperCase() : "?"}
        </div>
      </div>

      {/* Favorite star */}
      <button
        type="button"
        onClick={handleToggleFavorite}
        className="flex flex-shrink-0 items-center p-1"
      >
        <Star
          className={`size-4 ${
            isFav
              ? "fill-custom-amber text-custom-amber"
              : "text-custom-text-400 hover:text-custom-amber"
          }`}
        />
      </button>

      {/* Apply button */}
      <button
        type="button"
        onClick={handleApply}
        className="bg-custom-primary hover:bg-custom-primary/90 flex-shrink-0 rounded-md px-3 py-1.5 text-xs font-medium text-white"
      >
        应用
      </button>

      {/* Actions (edit / delete) */}
      <ViewListItemAction
        view={view}
        projectId={projectId}
        onEdit={() => {
          viewStore.openViewModal("edit");
        }}
        onDelete={() => {
          viewStore.openDeleteModal(view.id, view.name);
        }}
      />
    </div>
  );
});

export { ViewListItem };
