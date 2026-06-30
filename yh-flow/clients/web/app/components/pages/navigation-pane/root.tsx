// FLOW: Forked from Plane. Original: apps/web/core/components/pages/navigation-pane/root.tsx
// FLOW: PageNavigationPaneRoot — 右侧导航面板容器
"use client";
import { ArrowRightCircle } from "lucide-react";
import type { TPage } from "@plane/types";
import { PageNavigationPaneTabsList } from "./tabs-list";
import { PageNavigationPaneOutlineTabPanel } from "./tab-panels/outline";
import { PageNavigationPaneInfoTabPanel } from "./tab-panels/info/root";

const PANE_WIDTH = 280;

type Props = {
  isOpen: boolean;
  onClose: () => void;
  page: TPage;
};

export const PageNavigationPaneRoot = function PageNavigationPaneRoot({ isOpen, onClose, page }: Props) {
  return (
    <aside
      className="border-custom-border-200 bg-custom-background-90 flex h-full shrink-0 flex-col border-l pt-3.5 transition-all duration-300 ease-out"
      style={{
        width: `${PANE_WIDTH}px`,
        marginRight: isOpen ? "0px" : `-${PANE_WIDTH}px`,
      }}
    >
      <div className="mb-3.5 px-3.5">
        <button
          type="button"
          className="text-custom-text-400 hover:text-custom-text-100 grid size-3.5 place-items-center transition-colors"
          onClick={onClose}
          aria-label="关闭导航面板"
        >
          <ArrowRightCircle className="size-3.5" />
        </button>
      </div>

      {isOpen && (
        <div className="flex flex-1 flex-col overflow-hidden">
          <PageNavigationPaneTabsList />
          <div className="mt-4 flex-1 overflow-y-auto px-4">
            <PageNavigationPaneOutlineTabPanel page={page} />
            <div className="bg-custom-border-200 my-4 h-px" />
            <PageNavigationPaneInfoTabPanel page={page} />
          </div>
        </div>
      )}
    </aside>
  );
};
