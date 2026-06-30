// FLOW: Forked from Plane. Original: apps/web/core/components/views/filters/order-by.tsx
// FLOW: ViewOrderByDropdown — 视图排序下拉选择器
"use client";
import { useState, useRef, useEffect } from "react";
import { ArrowDownWideNarrow, ArrowUpWideNarrow, Check } from "lucide-react";
import type { TViewFiltersSortBy, TViewFiltersSortKey } from "@plane/types";

const SORT_KEY_OPTIONS: { key: TViewFiltersSortKey; label: string }[] = [
  { key: "name", label: "按名称排序" },
  { key: "created_at", label: "按创建时间排序" },
  { key: "updated_at", label: "按更新时间排序" },
];

const SORT_BY_OPTIONS: { key: TViewFiltersSortBy; label: string }[] = [
  { key: "asc", label: "升序" },
  { key: "desc", label: "降序" },
];

type Props = {
  sortKey: TViewFiltersSortKey;
  sortBy: TViewFiltersSortBy;
  onChange: (value: { key?: TViewFiltersSortKey; order?: TViewFiltersSortBy }) => void;
};

const ViewOrderByDropdown = function ViewOrderByDropdown({ sortKey, sortBy, onChange }: Props) {
  const [isOpen, setIsOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);
  const isDescending = sortBy === "desc";

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setIsOpen(false);
      }
    };
    if (isOpen) document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [isOpen]);

  const currentLabel = SORT_KEY_OPTIONS.find((o) => o.key === sortKey)?.label ?? "排序";

  return (
    <div ref={ref} className="relative">
      <button
        type="button"
        onClick={() => setIsOpen(!isOpen)}
        className="border-custom-border-200 text-custom-text-200 hover:bg-custom-background-80 flex items-center gap-1.5 rounded-md border px-2.5 py-1.5 text-xs"
      >
        {isDescending ? <ArrowDownWideNarrow className="size-3" /> : <ArrowUpWideNarrow className="size-3" />}
        <span>{currentLabel}</span>
      </button>

      {isOpen && (
        <div className="shadow-custom-shadow-2xs border-custom-border-200 bg-custom-background-90 absolute right-0 top-full z-10 mt-1 min-w-[180px] rounded-md border p-1">
          {/* Sort key options */}
          {SORT_KEY_OPTIONS.map((option) => (
            <button
              key={option.key}
              type="button"
              onClick={() => {
                onChange({ key: option.key });
                setIsOpen(false);
              }}
              className="hover:bg-custom-background-80 text-custom-text-200 flex w-full items-center justify-between rounded-sm px-2 py-1.5 text-xs"
            >
              {option.label}
              {sortKey === option.key && <Check className="size-3" />}
            </button>
          ))}
          <hr className="border-custom-border-200 my-1" />
          {/* Sort direction options */}
          {SORT_BY_OPTIONS.map((option) => {
            const isSelected = (option.key === "asc" && !isDescending) || (option.key === "desc" && isDescending);
            return (
              <button
                key={option.key}
                type="button"
                onClick={() => {
                  if (!isSelected) onChange({ order: option.key });
                  setIsOpen(false);
                }}
                className="hover:bg-custom-background-80 text-custom-text-200 flex w-full items-center justify-between rounded-sm px-2 py-1.5 text-xs"
              >
                {option.label}
                {isSelected && <Check className="size-3" />}
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
};

export { ViewOrderByDropdown };
