// FLOW: Forked from Plane. Original: apps/web/core/components/pages/editor/toolbar/root.tsx
// FLOW: PageEditorToolbarRoot — 工具栏容器
"use client";
import { PanelRight } from "lucide-react";

type Props = {
  isNavigationPaneOpen: boolean;
  onOpenNavigationPane: () => void;
};

export const PageEditorToolbarRoot = function PageEditorToolbarRoot({
  isNavigationPaneOpen,
  onOpenNavigationPane,
}: Props) {
  return (
    <div className="max-h-[52px] overflow-auto transition-all duration-300 ease-linear">
      <div className="page-toolbar-content relative flex min-h-[40px] items-center px-page-x transition-all duration-200 ease-in-out">
        <div className="flex w-full max-w-full items-center justify-between">
          <div className="flex-1" />
          <div className="flex items-center gap-2">
            {!isNavigationPaneOpen && (
              <button
                type="button"
                className="text-custom-text-400 hover:bg-custom-background-80 grid size-6 shrink-0 place-items-center rounded-sm transition-colors"
                onClick={onOpenNavigationPane}
                aria-label="打开导航面板"
              >
                <PanelRight className="size-3.5" />
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
