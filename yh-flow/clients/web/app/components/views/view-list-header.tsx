// FLOW: Forked from Plane. Original: apps/web/core/components/views/view-list-header.tsx
// FLOW: ViewListHeader — 视图列表头（搜索 + 排序 + 新建视图按钮）
"use client";
import { useEffect, useRef, useState } from "react";
import { observer } from "mobx-react";
import { Search, Plus } from "lucide-react";
import { useStore } from "@/lib/store-context";
import { ViewOrderByDropdown } from "./filters/order-by";

type Props = {
  onCreate: () => void;
};

const ViewListHeader = observer(function ViewListHeader({ onCreate }: Props) {
  const { view: viewStore } = useStore();
  const [isSearchOpen, setIsSearchOpen] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Escape") {
      if (viewStore.filters.searchQuery) {
        viewStore.setSearchQuery("");
      } else {
        setIsSearchOpen(false);
        inputRef.current?.blur();
      }
    }
  };

  useEffect(() => {
    if (viewStore.filters.searchQuery) setIsSearchOpen(true);
  }, [viewStore.filters.searchQuery]);

  return (
    <div className="flex items-center gap-2">
      {/* Search */}
      <div className="flex items-center">
        {!isSearchOpen && (
          <button
            type="button"
            onClick={() => {
              setIsSearchOpen(true);
              setTimeout(() => inputRef.current?.focus(), 50);
            }}
            className="text-custom-text-400 hover:text-custom-text-200 rounded p-2"
          >
            <Search className="size-3.5" />
          </button>
        )}
        <div
          className={`flex items-center gap-1 overflow-hidden rounded-md border transition-all ${
            isSearchOpen
              ? "border-custom-border-200 bg-custom-background-90 w-64 px-2.5 py-1.5 opacity-100"
              : "w-0 border-transparent px-0 py-0 opacity-0"
          }`}
        >
          <Search className="size-3.5 flex-shrink-0 text-custom-text-400" />
          <input
            ref={inputRef}
            type="text"
            value={viewStore.filters.searchQuery}
            onChange={(e) => viewStore.setSearchQuery(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder="搜索视图..."
            className="w-full border-none bg-transparent text-sm text-custom-text-100 outline-none placeholder:text-custom-text-400"
          />
        </div>
      </div>

      {/* Sort dropdown */}
      <ViewOrderByDropdown
        sortKey={viewStore.filters.sortKey}
        sortBy={viewStore.filters.sortBy}
        onChange={(value) => {
          if (value.key) viewStore.setSortKey(value.key as any);
          if (value.order) viewStore.setSortBy(value.order as any);
        }}
      />

      {/* Create button */}
      <button
        type="button"
        onClick={onCreate}
        className="bg-custom-primary hover:bg-custom-primary/90 flex items-center gap-1.5 rounded-md px-3 py-2 text-xs font-medium text-white"
      >
        <Plus className="size-3.5" />
        新建视图
      </button>
    </div>
  );
});

export { ViewListHeader };
