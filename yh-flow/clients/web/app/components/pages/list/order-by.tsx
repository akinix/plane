// FLOW: Forked from Plane. Original: apps/web/core/components/pages/list/order-by.tsx
// FLOW: PageOrderByDropdown — 排序下拉菜单
"use client";
import { ArrowDownWideNarrow, ArrowUpWideNarrow, Check } from "lucide-react";
import type { TPageFiltersSortBy, TPageFiltersSortKey } from "@plane/types";
import { CustomMenu } from "@plane/ui";
import { cn } from "@plane/utils";

type Props = {
  onChange: (value: { key?: TPageFiltersSortKey; order?: TPageFiltersSortBy }) => void;
  sortBy: TPageFiltersSortBy;
  sortKey: TPageFiltersSortKey;
};

const PAGE_SORTING_KEY_OPTIONS: {
  key: TPageFiltersSortKey;
  label: string;
}[] = [
  { key: "name", label: "按名称排序" },
  { key: "created_at", label: "按创建时间排序" },
  { key: "updated_at", label: "按更新时间排序" },
];

export function PageOrderByDropdown(props: Props) {
  const { onChange, sortBy, sortKey } = props;

  const orderByDetails = PAGE_SORTING_KEY_OPTIONS.find((option) => sortKey === option.key);
  const isDescending = sortBy === "desc";

  return (
    <CustomMenu
      customButton={
        <div
          className={cn(
            "text-sm text-custom-text-300 flex items-center gap-1.5 rounded-md px-2.5 py-1.5 transition-colors",
            "hover:bg-custom-background-80 cursor-pointer"
          )}
        >
          {!isDescending ? <ArrowUpWideNarrow className="size-3.5" /> : <ArrowDownWideNarrow className="size-3.5" />}
          {orderByDetails?.label ?? "按名称排序"}
        </div>
      }
      placement="bottom-end"
      maxHeight="lg"
      closeOnSelect
    >
      {PAGE_SORTING_KEY_OPTIONS.map((option) => (
        <CustomMenu.MenuItem
          key={option.key}
          className="flex items-center justify-between gap-2"
          onClick={() => onChange({ key: option.key })}
        >
          {option.label}
          {sortKey === option.key && <Check className="text-custom-primary size-3" />}
        </CustomMenu.MenuItem>
      ))}
      <hr className="border-custom-border-200 my-2" />
      <CustomMenu.MenuItem
        className="flex items-center justify-between gap-2"
        onClick={() => onChange({ order: "asc" })}
      >
        升序
        {!isDescending && <Check className="text-custom-primary size-3" />}
      </CustomMenu.MenuItem>
      <CustomMenu.MenuItem
        className="flex items-center justify-between gap-2"
        onClick={() => onChange({ order: "desc" })}
      >
        降序
        {isDescending && <Check className="text-custom-primary size-3" />}
      </CustomMenu.MenuItem>
    </CustomMenu>
  );
}
