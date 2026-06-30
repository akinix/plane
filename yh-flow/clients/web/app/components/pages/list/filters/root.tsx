// FLOW: Forked from Plane. Original: apps/web/core/components/pages/list/filters/root.tsx
// FLOW: PageFiltersSelection — 筛选器容器（预留扩展）
"use client";
import type { TPageFilters } from "@plane/types";

type Props = {
  filters: TPageFilters;
  handleFiltersUpdate: <T extends keyof TPageFilters>(filterKey: T, filterValue: TPageFilters[T]) => void;
};

export function PageFiltersSelection(props: Props) {
  const { filters: _filters, handleFiltersUpdate: _handleFiltersUpdate } = props;

  return (
    <div className="flex h-full w-full flex-col overflow-hidden">
      <div className="text-sm text-custom-text-300 px-2.5 py-2">筛选条件（预留扩展）</div>
    </div>
  );
}
