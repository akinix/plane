// FLOW: Forked from Plane. Original: apps/web/core/components/pages/navigation-pane/tabs-list.tsx
// FLOW: PageNavigationPaneTabsList — Tab 导航（大纲 / 信息）
"use client";
import { useState } from "react";
import { cn } from "@plane/utils";

const TABS = [
  { key: "outline", label: "大纲" },
  { key: "info", label: "信息" },
];

type Props = {
  activeTab?: string;
  onTabChange?: (tab: string) => void;
};

export const PageNavigationPaneTabsList = function PageNavigationPaneTabsList({
  activeTab: externalTab,
  onTabChange,
}: Props) {
  const [internalTab, setInternalTab] = useState("outline");
  const activeTab = externalTab ?? internalTab;

  const handleTabChange = (tab: string) => {
    setInternalTab(tab);
    onTabChange?.(tab);
  };

  return (
    <div className="border-custom-border-200 mx-3.5 flex gap-2 border-b">
      {TABS.map((tab) => (
        <button
          key={tab.key}
          type="button"
          onClick={() => handleTabChange(tab.key)}
          className={cn(
            "text-xs px-3 py-2 font-medium transition-colors",
            activeTab === tab.key
              ? "text-custom-text-100 border-custom-primary border-b-2"
              : "text-custom-text-400 hover:text-custom-text-300"
          )}
        >
          {tab.label}
        </button>
      ))}
    </div>
  );
};
