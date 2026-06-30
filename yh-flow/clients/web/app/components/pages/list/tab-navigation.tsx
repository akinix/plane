// FLOW: Forked from Plane. Original: apps/web/core/components/pages/list/tab-navigation.tsx
// FLOW: PageTabNavigation — Tab 导航（公开/私人/已归档）
"use client";
import type { TPageNavigationTabs } from "@plane/types";
import { cn } from "@plane/utils";

type Props = {
  workspaceId: string;
  activeTab: TPageNavigationTabs;
  onTabChange: (tab: TPageNavigationTabs) => void;
};

const PAGE_TABS: { key: TPageNavigationTabs; label: string }[] = [
  { key: "public", label: "公开" },
  { key: "private", label: "私人" },
  { key: "archived", label: "已归档" },
];

export function PageTabNavigation({ workspaceId: _workspaceId, activeTab, onTabChange }: Props) {
  return (
    <div className="flex h-full items-center gap-0">
      {PAGE_TABS.map((tab) => (
        <button
          key={tab.key}
          onClick={() => onTabChange(tab.key)}
          className={cn("text-sm flex items-center justify-center px-4 py-3 font-medium transition-all", {
            "text-custom-primary border-custom-primary border-b-2": tab.key === activeTab,
            "text-custom-text-300 hover:text-custom-text-200": tab.key !== activeTab,
          })}
        >
          {tab.label}
        </button>
      ))}
    </div>
  );
}
