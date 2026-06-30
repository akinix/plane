// FLOW: Forked from Plane. Original: apps/web/core/components/views/applied-filters/root.tsx
// FLOW: ViewAppliedFiltersList — 已应用筛选器标签展示
"use client";
import { X } from "lucide-react";
import type { TViewFilterProps } from "@plane/types";

type Props = {
  appliedFilters: TViewFilterProps;
  onClearAll: () => void;
  onRemoveFilter: (key: keyof TViewFilterProps, value?: string | null) => void;
};

const ViewAppliedFiltersList = function ViewAppliedFiltersList({ appliedFilters, onClearAll, onRemoveFilter }: Props) {
  if (!appliedFilters || Object.keys(appliedFilters).length === 0) return null;

  const entries = Object.entries(appliedFilters).filter(([, value]) => {
    if (!value) return false;
    if (Array.isArray(value) && value.length === 0) return false;
    return true;
  });

  if (entries.length === 0) return null;

  return (
    <div className="flex flex-wrap items-center gap-2 px-4 py-2">
      {entries.map(([key]) => (
        <span
          key={key}
          className="bg-custom-background-80 border-custom-border-200 flex items-center gap-1 rounded-sm border px-2 py-1 text-2xs text-custom-text-300"
        >
          {key}
          <button type="button" onClick={() => onRemoveFilter(key as keyof TViewFilterProps, null)}>
            <X className="size-3" />
          </button>
        </span>
      ))}
      <button
        type="button"
        onClick={onClearAll}
        className="bg-custom-background-80 border-custom-border-200 rounded-sm border px-2 py-1 text-2xs text-custom-text-300"
      >
        清除全部
      </button>
    </div>
  );
};

export { ViewAppliedFiltersList };
