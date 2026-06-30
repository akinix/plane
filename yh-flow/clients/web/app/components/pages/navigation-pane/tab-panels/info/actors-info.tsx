// FLOW: Forked from Plane. Original: apps/web/core/components/pages/navigation-pane/tab-panels/info/actors-info.tsx
// FLOW: PageNavigationPaneInfoTabActorsInfo — 参与者信息
"use client";
import { useMemo } from "react";
import type { TPage } from "@plane/types";

type Props = {
  page: TPage;
};

// Simple user lookup (mock — uses display_name from mock members)
const MOCK_USERS: Record<string, { display_name: string }> = {
  "user-1": { display_name: "张三" },
  "user-2": { display_name: "李四" },
  "user-3": { display_name: "王五" },
  "user-4": { display_name: "赵六" },
  "user-5": { display_name: "陈七" },
};

export const PageNavigationPaneInfoTabActorsInfo = function PageNavigationPaneInfoTabActorsInfo({ page }: Props) {
  const owner = MOCK_USERS[page.owned_by]?.display_name || page.owned_by;

  const actors = useMemo(
    () => [
      { label: "所有者", name: owner },
      { label: "创建者", name: MOCK_USERS[page.created_by]?.display_name || page.created_by },
      { label: "最后编辑者", name: MOCK_USERS[page.updated_by]?.display_name || page.updated_by },
    ],
    [owner, page.created_by, page.updated_by]
  );

  return (
    <div className="space-y-3">
      <h4 className="text-xs text-custom-text-400 font-semibold">参与者</h4>
      <div className="space-y-2">
        {actors.map((actor) => (
          <div key={actor.label} className="flex flex-col">
            <span className="text-xs text-custom-text-400">{actor.label}</span>
            <span className="text-sm text-custom-text-200">{actor.name}</span>
          </div>
        ))}
      </div>
    </div>
  );
};
