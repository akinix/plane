// FLOW: Forked from Plane. Original: apps/web/core/components/views/views-list.tsx
// FLOW: ViewsList — 视图列表主容器（Tab + 搜索 + 排序 + 卡片列表）
"use client";
import { useMemo } from "react";
import { observer } from "mobx-react";
import { useNavigate } from "react-router";
import { useStore } from "@/lib/store-context";
import { useViews } from "@/../src/lib/hooks/use-views";
import type { TIssueView } from "@/components/issues/filters/types";
import { ViewListHeader } from "./view-list-header";
import { ViewListItem } from "./view-list-item";
import { ViewModal } from "./modal";
import { DeleteViewModal } from "./delete-view-modal";

type Props = {
  workspaceId: string;
  projectId: string;
};

const ViewsList = observer(function ViewsList({ workspaceId, projectId }: Props) {
  const navigate = useNavigate();
  const { view: viewStore } = useStore();
  const { data: views, isLoading, isError } = useViews(projectId);

  // Filter views by active tab
  const tabFiltered = useMemo(() => {
    if (!views) return [];
    if (viewStore.activeTab === "created") {
      return views.filter((v) => (v as any).created_by === "user-1");
    }
    return views;
  }, [views, viewStore.activeTab]);

  // Client-side search filter
  const searched = useMemo(() => {
    if (!viewStore.filters.searchQuery) return tabFiltered;
    const q = viewStore.filters.searchQuery.toLowerCase();
    return tabFiltered.filter((v) => v.name?.toLowerCase().includes(q));
  }, [tabFiltered, viewStore.filters.searchQuery]);

  // Client-side sort
  const sorted = useMemo(() => {
    return [...searched].toSorted((a: TIssueView, b: TIssueView) => {
      const sortKey = viewStore.filters.sortKey;
      const sortBy = viewStore.filters.sortBy;
      let cmp = 0;
      if (sortKey === "name") {
        cmp = (a.name ?? "").localeCompare(b.name ?? "");
      } else if (sortKey === "created_at") {
        cmp = new Date(a.createdAt ?? 0).getTime() - new Date(b.createdAt ?? 0).getTime();
      } else if (sortKey === "updated_at") {
        cmp = new Date(a.updatedAt ?? 0).getTime() - new Date(b.updatedAt ?? 0).getTime();
      }
      return sortBy === "desc" ? -cmp : cmp;
    });
  }, [searched, viewStore.filters.sortKey, viewStore.filters.sortBy]);

  const handleCreate = () => {
    viewStore.openViewModal("create");
  };

  if (isLoading) {
    return (
      <div className="flex h-full flex-col overflow-hidden">
        <div className="border-custom-border-200 flex items-center justify-between border-b px-4 py-3">
          <div className="h-6 w-48 animate-pulse rounded bg-custom-background-80" />
          <div className="flex items-center gap-2">
            <div className="h-8 w-32 animate-pulse rounded bg-custom-background-80" />
            <div className="h-8 w-24 animate-pulse rounded bg-custom-background-80" />
          </div>
        </div>
        <div className="flex-1 space-y-2 overflow-y-auto p-4">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-16 animate-pulse rounded bg-custom-background-80" />
          ))}
        </div>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="flex h-full flex-col items-center justify-center gap-3">
        <p className="text-sm text-custom-text-300">加载视图列表失败</p>
      </div>
    );
  }

  return (
    <div className="flex h-full flex-col overflow-hidden">
      {/* Tab + Search + Order bar */}
      <div className="border-custom-border-200 flex items-center justify-between border-b px-4">
        <div className="flex items-center gap-0">
          <button
            type="button"
            onClick={() => viewStore.setActiveTab("all")}
            className={`px-4 py-3 text-sm font-medium transition-colors ${
              viewStore.activeTab === "all"
                ? "border-custom-primary border-b-2 text-custom-text-100"
                : "text-custom-text-300 hover:text-custom-text-200"
            }`}
          >
            全部
          </button>
          <button
            type="button"
            onClick={() => viewStore.setActiveTab("created")}
            className={`px-4 py-3 text-sm font-medium transition-colors ${
              viewStore.activeTab === "created"
                ? "border-custom-primary border-b-2 text-custom-text-100"
                : "text-custom-text-300 hover:text-custom-text-200"
            }`}
          >
            我创建的
          </button>
        </div>
        <ViewListHeader onCreate={handleCreate} />
      </div>

      {/* View list */}
      <div className="flex-1 overflow-y-auto">
        {sorted.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-20">
            <h3 className="text-lg font-medium text-custom-text-100">暂无视图</h3>
            <p className="mt-1 text-sm text-custom-text-300">
              在 Issue 视图中保存当前配置即可创建视图。
            </p>
          </div>
        ) : (
          <div className="divide-custom-border-200 divide-y">
            {sorted.map((view) => (
              <ViewListItem
                key={view.id}
                view={view}
                workspaceId={workspaceId}
                projectId={projectId}
              />
            ))}
          </div>
        )}
      </div>

      {/* Modals */}
      <ViewModal
        isOpen={viewStore.viewModalOpen}
        onClose={() => viewStore.closeViewModal()}
        projectId={projectId}
        workspaceId={workspaceId}
      />
      {viewStore.deleteViewId && (
        <DeleteViewModal
          isOpen={viewStore.viewDeleting}
          onClose={() => viewStore.closeDeleteModal()}
          viewId={viewStore.deleteViewId}
          viewName={viewStore.deleteViewName ?? ""}
          projectId={projectId}
        />
      )}
    </div>
  );
});

export { ViewsList };
