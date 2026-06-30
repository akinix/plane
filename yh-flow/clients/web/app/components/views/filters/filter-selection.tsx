// FLOW: Forked from Plane. Original: apps/web/core/components/views/filters/filter-selection.tsx
// FLOW: ViewFiltersSelection — 视图列表筛选面板（按创建者、视图类型等）
"use client";
import { useState } from "react";
import { observer } from "mobx-react";
import { Search } from "lucide-react";
import type { TViewFilters, TViewFilterProps } from "@plane/types";
import { EViewAccess } from "@plane/types";

type Props = {
  filters: TViewFilters;
  onFiltersUpdate: (filters: TViewFilters) => void;
};

const ViewFiltersSelection = observer(function ViewFiltersSelection({ filters, onFiltersUpdate }: Props) {
  const [searchQuery, setSearchQuery] = useState("");

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setSearchQuery(value);
    onFiltersUpdate({
      ...filters,
      searchQuery: value,
    });
  };

  const handleFilterToggle = (key: keyof TViewFilterProps, value: string | EViewAccess) => {
    const current = filters.filters?.[key] ?? [];
    const arr = Array.isArray(current) ? [...current] : [];
    const idx = arr.indexOf(value as never);
    if (idx >= 0) {
      arr.splice(idx, 1);
    } else {
      arr.push(value as never);
    }
    onFiltersUpdate({
      ...filters,
      filters: {
        ...filters.filters,
        [key]: arr,
      },
    });
  };

  return (
    <div className="flex flex-col">
      <div className="border-custom-border-200 bg-custom-background-90 flex items-center gap-1.5 border-b p-2.5">
        <Search className="size-3 text-custom-text-400" />
        <input
          type="text"
          value={searchQuery}
          onChange={handleSearchChange}
          placeholder="搜索筛选条件..."
          className="w-full bg-transparent text-xs text-custom-text-100 outline-none placeholder:text-custom-text-400"
        />
      </div>
      <div className="p-2.5">
        {/* View type filter */}
        <div className="mb-3">
          <span className="mb-1.5 block text-xs font-medium text-custom-text-300">视图类型</span>
          <div className="flex flex-col gap-1">
            <label className="flex items-center gap-2 text-xs text-custom-text-200">
              <input
                type="checkbox"
                checked={((filters.filters?.view_type ?? []) as EViewAccess[]).includes(EViewAccess.PUBLIC)}
                onChange={() => handleFilterToggle("view_type", EViewAccess.PUBLIC)}
                className="rounded border-custom-border-200"
              />
              公开
            </label>
            <label className="flex items-center gap-2 text-xs text-custom-text-200">
              <input
                type="checkbox"
                checked={((filters.filters?.view_type ?? []) as EViewAccess[]).includes(EViewAccess.PRIVATE)}
                onChange={() => handleFilterToggle("view_type", EViewAccess.PRIVATE)}
                className="rounded border-custom-border-200"
              />
              私人
            </label>
          </div>
        </div>
      </div>
    </div>
  );
});

export { ViewFiltersSelection };
