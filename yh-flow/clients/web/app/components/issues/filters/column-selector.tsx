// FLOW: ColumnSelector — column visibility popup for all 5 views (per D-P17-13)
"use client";

import { useCallback } from "react";
import { RotateCcw } from "lucide-react";
import { cn } from "@plane/utils";

const DEFAULT_COLUMNS = ["state", "priority", "assignee"] as const;

const ALL_AVAILABLE_COLUMNS: { key: string; label: string }[] = [
  { key: "state", label: "状态" },
  { key: "priority", label: "优先级" },
  { key: "assignee", label: "负责人" },
  { key: "start_date", label: "开始日期" },
  { key: "target_date", label: "截止日期" },
  { key: "labels", label: "标签" },
  { key: "cycle", label: "周期" },
  { key: "module", label: "模块" },
  { key: "estimate", label: "估算" },
  { key: "created_at", label: "创建时间" },
  { key: "updated_at", label: "更新时间" },
  { key: "attachments", label: "附件数" },
  { key: "links", label: "链接数" },
  { key: "sub_issues", label: "子 Issue 数" },
];

type TColumnSelectorProps = {
  isOpen: boolean;
  onClose: () => void;
  selectedColumns: string[];
  onChange: (columns: string[]) => void;
};

export function ColumnSelector({ isOpen, onClose, selectedColumns, onChange }: TColumnSelectorProps) {
  const handleToggle = useCallback(
    (key: string) => {
      if (selectedColumns.includes(key)) {
        onChange(selectedColumns.filter((c) => c !== key));
      } else {
        onChange([...selectedColumns, key]);
      }
    },
    [selectedColumns, onChange]
  );

  const handleReset = useCallback(() => {
    onChange(DEFAULT_COLUMNS.map((c) => c));
  }, [onChange]);

  if (!isOpen) return null;

  return (
    <>
      {/* Backdrop */}
      <div className="fixed inset-0 z-20" onClick={onClose} aria-hidden="true" />

      {/* Overlay panel */}
      <div className="border-custom-border-200 bg-custom-background-100 shadow-lg absolute top-full right-0 z-20 mt-1 w-56 origin-top-right rounded-md border">
        {/* Header */}
        <div className="border-custom-border-200 flex items-center justify-between border-b px-3 py-2">
          <span className="text-xs text-custom-text-200 font-medium">显示列</span>
          <button
            type="button"
            onClick={handleReset}
            className="text-xs text-custom-text-400 hover:text-custom-text-200 flex items-center gap-1"
          >
            <RotateCcw className="size-3" />
            重置
          </button>
        </div>

        {/* Column list */}
        <div className="max-h-64 overflow-y-auto py-1">
          {ALL_AVAILABLE_COLUMNS.map((col) => {
            const isChecked = selectedColumns.includes(col.key);
            return (
              <label
                key={col.key}
                className={cn(
                  "text-xs hover:bg-custom-background-80 flex cursor-pointer items-center gap-2 px-3 py-1.5 transition-colors",
                  isChecked ? "text-custom-text-100" : "text-custom-text-300"
                )}
              >
                <input
                  type="checkbox"
                  checked={isChecked}
                  onChange={() => handleToggle(col.key)}
                  className="border-custom-border-200 text-custom-primary focus:ring-custom-primary size-3.5 rounded"
                />
                <span>{col.label}</span>
              </label>
            );
          })}
        </div>

        {/* Footer */}
        <div className="border-custom-border-200 border-t px-3 py-2">
          <button
            type="button"
            onClick={onClose}
            className="bg-custom-primary text-xs hover:bg-custom-primary/90 w-full rounded-md px-3 py-1.5 font-medium text-white"
          >
            确定
          </button>
        </div>
      </div>
    </>
  );
}
