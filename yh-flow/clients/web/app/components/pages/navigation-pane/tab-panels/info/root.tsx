// FLOW: Forked from Plane. Original: apps/web/core/components/pages/navigation-pane/tab-panels/info/root.tsx
// FLOW: PageNavigationPaneInfoTabPanel — 信息面板容器
"use client";
import type { TPage } from "@plane/types";
import { PageNavigationPaneInfoTabDocumentInfo } from "./document-info";
import { PageNavigationPaneInfoTabActorsInfo } from "./actors-info";

type Props = {
  page: TPage;
};

export const PageNavigationPaneInfoTabPanel = function PageNavigationPaneInfoTabPanel({ page }: Props) {
  return (
    <div className="flex h-full flex-col">
      <div className="mt-3 flex-1 overflow-y-auto">
        <PageNavigationPaneInfoTabDocumentInfo page={page} />
        <div className="bg-custom-border-200 my-3 h-px" />
        <PageNavigationPaneInfoTabActorsInfo page={page} />
      </div>
    </div>
  );
};
