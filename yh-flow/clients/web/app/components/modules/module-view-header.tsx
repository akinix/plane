// FLOW: Forked from Plane modules/module-view-header.tsx
// FLOW: ModuleViewHeader — Module 列表页头（标题 + 创建按钮 + 视图切换）
import React, { useRef, useState } from "react";
import { ListFilter, Search, X, LayoutGrid, List } from "lucide-react";
import { cn } from "@plane/utils";
import { useStore } from "@/lib/store-context";
import { ModuleLayoutIcon } from "./module-layout-icon";

type Props = {
  projectId: string;
};

type TViewLayout = "board" | "list";

const VIEW_LAYOUTS: { key: TViewLayout; label: string }[] = [
  { key: "board", label: "网格" },
  { key: "list", label: "列表" },
];

export const ModuleViewHeader = React.memo(function ModuleViewHeader({ projectId }: Props) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [isSearchOpen, setIsSearchOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");

  // Access ModuleStore via root store
  const { module: moduleStore } = useStore();
  const activeView: TViewLayout = (moduleStore as any)?.activeView ?? "board";

  const handleInputKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Escape") {
      if (searchQuery && searchQuery.trim() !== "") setSearchQuery("");
      else {
        setIsSearchOpen(false);
        inputRef.current?.blur();
      }
    }
  };

  return (
    <div className="flex items-center justify-between px-4 py-2.5">
      {/* 左侧：标题 */}
      <h2 className="text-18 font-semibold text-primary">模块</h2>

      {/* 右侧：操作 */}
      <div className="flex items-center gap-2">
        {/* 搜索 */}
        <div className="flex items-center">
          {!isSearchOpen && (
            <button
              className="grid place-items-center p-1 text-tertiary hover:text-primary"
              onClick={() => {
                setIsSearchOpen(true);
                inputRef.current?.focus();
              }}
            >
              <Search className="h-3.5 w-3.5" />
            </button>
          )}
          <div
            className={cn(
              "flex items-center gap-1 overflow-hidden rounded-md border border-transparent bg-surface-1 transition-all",
              isSearchOpen ? "w-48 border-subtle px-2 py-1" : "w-0"
            )}
          >
            <input
              ref={inputRef}
              className="w-full border-none bg-transparent text-13 text-primary outline-none"
              placeholder="搜索模块..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              onKeyDown={handleInputKeyDown}
            />
            {isSearchOpen && (
              <button onClick={() => setIsSearchOpen(false)}>
                <X className="h-3 w-3" />
              </button>
            )}
          </div>
        </div>

        {/* 视图切换 */}
        <div className="flex items-center gap-1 rounded-sm bg-layer-3 p-0.5">
          {VIEW_LAYOUTS.map((layout) => (
            <button
              key={layout.key}
              className={cn(
                "grid h-6 w-7 place-items-center rounded-sm transition-all",
                activeView === layout.key ? "bg-surface-1 shadow-sm" : "hover:bg-surface-1/50"
              )}
              onClick={() => {
                /* ModuleStore handles layout change */
              }}
            >
              {layout.key === "board" ? (
                <LayoutGrid className="h-3.5 w-3.5" />
              ) : (
                <List className="h-3.5 w-3.5" />
              )}
            </button>
          ))}
        </div>

        {/* 创建按钮 */}
        <button
          className="rounded-md bg-accent-primary px-3 py-1.5 text-13 font-medium text-white hover:bg-accent-primary/90"
          onClick={() => moduleStore.openModuleModal("create")}
        >
          创建模块
        </button>
      </div>
    </div>
  );
});
