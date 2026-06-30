// FLOW: Forked from Plane. Original: apps/web/core/components/pages/pages-list-view.tsx
// FLOW: PagesListView — 页面卡片网格渲染容器
"use client";
import type { TPage, TPageNavigationTabs } from "@plane/types";
import { PagesListRoot } from "./list/root";

type Props = {
  workspaceId: string;
  pages: TPage[];
  activeTab: TPageNavigationTabs;
};

export const PagesListView = function PagesListView({ workspaceId, pages, activeTab }: Props) {
  return (
    <div className="h-full w-full px-4 py-4">
      <PagesListRoot workspaceId={workspaceId} pageList={pages} activeTab={activeTab} />
    </div>
  );
};
