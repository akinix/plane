// FLOW: Forked from Plane. Original: apps/web/core/components/pages/list/root.tsx
// FLOW: PagesListRoot — 页面列表核心渲染逻辑
"use client";
import type { TPage, TPageNavigationTabs } from "@plane/types";
import { PageListBlock } from "./block";

type Props = {
  workspaceId: string;
  pageList: TPage[];
  activeTab: TPageNavigationTabs;
};

export const PagesListRoot = function PagesListRoot({ workspaceId, pageList, activeTab }: Props) {
  // Empty state by tab
  if (!pageList || pageList.length === 0) {
    const emptyMessages: Record<TPageNavigationTabs, { title: string; description: string }> = {
      public: {
        title: "暂无公开页面",
        description: "公开页面可以被工作区内所有人查看和编辑。",
      },
      private: {
        title: "暂无私人页面",
        description: "私人页面仅创建者可查看。",
      },
      archived: {
        title: "没有已归档的页面",
        description: "归档的页面将在此处显示。",
      },
    };

    const msg = emptyMessages[activeTab];

    return (
      <div className="flex flex-col items-center justify-center py-20">
        <h3 className="text-lg text-custom-text-100 font-medium">{msg.title}</h3>
        <p className="text-sm text-custom-text-300 mt-1">{msg.description}</p>
      </div>
    );
  }

  return (
    <div className="3xl:grid-cols-4 grid grid-cols-1 gap-4 lg:grid-cols-2 xl:grid-cols-3">
      {pageList.map((page) => (
        <PageListBlock key={page.id} page={page} workspaceId={workspaceId} />
      ))}
    </div>
  );
};
