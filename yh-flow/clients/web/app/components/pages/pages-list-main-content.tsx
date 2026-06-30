// FLOW: Forked from Plane. Original: apps/web/core/components/pages/pages-list-main-content.tsx
// FLOW: PagesListMainContent — 页面列表主容器（Tab + 搜索 + 排序 + 网格）
"use client";
import { observer } from "mobx-react";
import { useStore } from "@/lib/store-context";
import { usePages } from "@/../src/lib/hooks/use-pages";
import { EPageAccess, type TPage, type TPageNavigationTabs } from "@plane/types";
import { PagesListView } from "./pages-list-view";
import { PageTabNavigation } from "./list/tab-navigation";
import { PageSearchInput } from "./list/search-input";
import { PageOrderByDropdown } from "./list/order-by";
import { PageContentLoader } from "./loaders/page-content-loader";

type Props = {
  workspaceId: string;
};

export const PagesListMainContent = observer(function PagesListMainContent({ workspaceId }: Props) {
  // Store
  const { page: pageStore } = useStore();
  const activeTab = pageStore.activeTab;
  const filters = pageStore.filters;

  // Data
  const { data: pages, isLoading, isError } = usePages(workspaceId);

  // Filter pages based on active tab
  const filteredByTab =
    pages?.filter((p: TPage) => {
      if (activeTab === "public") return p.access === EPageAccess.PUBLIC && !p.archived_at;
      if (activeTab === "private") return p.access === EPageAccess.PRIVATE && !p.archived_at;
      if (activeTab === "archived") return !!p.archived_at;
      return true;
    }) ?? [];

  // Client-side search filter
  const searched = filters.searchQuery
    ? filteredByTab.filter((p: TPage) => p.name?.toLowerCase().includes(filters.searchQuery.toLowerCase()))
    : filteredByTab;

  // Client-side sort
  const sorted = [...searched].toSorted((a: TPage, b: TPage) => {
    const sortKey = filters.sortKey;
    const sortBy = filters.sortBy;
    let cmp = 0;
    if (sortKey === "name") {
      cmp = (a.name ?? "").localeCompare(b.name ?? "");
    } else if (sortKey === "created_at") {
      const aTime = a.created_at ? new Date(a.created_at).getTime() : 0;
      const bTime = b.created_at ? new Date(b.created_at).getTime() : 0;
      cmp = aTime - bTime;
      if (isNaN(cmp)) cmp = 0;
    } else if (sortKey === "updated_at") {
      const aTime = a.updated_at ? new Date(a.updated_at).getTime() : 0;
      const bTime = b.updated_at ? new Date(b.updated_at).getTime() : 0;
      cmp = aTime - bTime;
      if (isNaN(cmp)) cmp = 0;
    }
    return sortBy === "desc" ? -cmp : cmp;
  });

  // Loading state
  if (isLoading) {
    return <PageContentLoader />;
  }

  // Error state
  if (isError) {
    return (
      <div className="flex h-full flex-col items-center justify-center gap-3">
        <p className="text-sm text-custom-text-300">加载页面列表失败</p>
      </div>
    );
  }

  return (
    <div className="flex h-full flex-col overflow-hidden">
      {/* Tab + Search + Order bar */}
      <div className="border-custom-border-200 flex items-center justify-between overflow-x-auto border-b px-4">
        <PageTabNavigation
          workspaceId={workspaceId}
          activeTab={activeTab}
          onTabChange={(tab: TPageNavigationTabs) => pageStore.setActiveTab(tab)}
        />
        <div className="flex items-center gap-2">
          <PageSearchInput
            searchQuery={filters.searchQuery}
            updateSearchQuery={(val: string) => pageStore.setSearchQuery(val)}
          />
          <PageOrderByDropdown
            sortKey={filters.sortKey}
            sortBy={filters.sortBy}
            onChange={(value: { key?: string; order?: string }) => {
              if (value.key) pageStore.setSortKey(value.key as any);
              if (value.order) pageStore.setSortBy(value.order as any);
            }}
          />
        </div>
      </div>

      {/* Page grid */}
      <div className="flex-1 overflow-y-auto">
        <PagesListView workspaceId={workspaceId} pages={sorted} activeTab={activeTab} />
      </div>
    </div>
  );
});
