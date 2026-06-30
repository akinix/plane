// FLOW: Forked from Plane. Original: apps/web/core/components/pages/list/applied-filters/root.tsx
// FLOW: PageAppliedFiltersList — 已应用筛选器（预留扩展）
"use client";
import type { TPageFilterProps } from "@plane/types";

type Props = {
  appliedFilters: TPageFilterProps;
  handleClearAllFilters: () => void;
  handleRemoveFilter: (key: keyof TPageFilterProps, value: string | null) => void;
};

export function PageAppliedFiltersList(props: Props) {
  const { appliedFilters } = props;

  if (!appliedFilters) return null;
  if (Object.keys(appliedFilters).length === 0) return null;

  return <div className="flex flex-wrap items-stretch gap-2 px-4">{/* Applied filter tags — 预留扩展 */}</div>;
}
