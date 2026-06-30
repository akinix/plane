// FLOW: FilterSaveModal — save filter/sort/column/layout configuration as a named view (per D-P17-14, FILT-04, D-P17-15)
"use client";

import { useState } from "react";
import { X } from "lucide-react";
import { ModalCore, EModalWidth } from "@plane/ui";
import { useCreateIssueView } from "@/../src/lib/hooks/use-issue-views";
import type { TFilterCriteria, TSortConfig, TViewLayout } from "./types";

const LAYOUT_LABELS: Record<TViewLayout, string> = {
  list: "列表",
  kanban: "看板",
  calendar: "日历",
  gantt: "甘特图",
  spreadsheet: "电子表格",
};

type TFilterSaveModalProps = {
  isOpen: boolean;
  onClose: () => void;
  projectId: string;
  workspaceId: string;
  currentFilters: TFilterCriteria;
  currentSort: TSortConfig;
  currentColumns: string[];
  currentLayout: TViewLayout;
  onSave?: () => void; // FLOW: post-save navigation callback (19-04 VIEW-02)
};

export function FilterSaveModal({
  isOpen,
  onClose,
  projectId,
  workspaceId: _workspaceId,
  currentFilters,
  currentSort,
  currentColumns,
  currentLayout,
  onSave,
}: TFilterSaveModalProps) {
  const [name, setName] = useState("");
  const { mutateAsync: createIssueView, isPending } = useCreateIssueView();

  const handleSave = async () => {
    if (!name.trim()) return;

    try {
      await createIssueView({
        name: name.trim(),
        projectId,
        filters: currentFilters,
        sort: currentSort,
        groupBy: "state",
        subGroupBy: "none",
        displayColumns: currentColumns,
        layout: currentLayout,
      });
      setName("");
      onClose();
      onSave?.();
      // FUTURE: show toast notification when toast system is available
    } catch (error) {
      console.error("[FilterSaveModal] Failed to save view:", error);
    }
  };

  const handleClose = () => {
    setName("");
    onClose();
  };

  return (
    <ModalCore isOpen={isOpen} handleClose={handleClose} width={EModalWidth.MD}>
      <div className="flex flex-col">
        {/* Header */}
        <div className="border-custom-border-200 flex items-center justify-between border-b px-5 py-4">
          <span className="text-base text-custom-text-100 font-semibold">保存为视图</span>
          <button type="button" onClick={handleClose} className="text-custom-text-400 hover:text-custom-text-200">
            <X className="size-4" />
          </button>
        </div>

        {/* Body */}
        <div className="flex flex-col gap-4 px-5 py-4">
          {/* Name input */}
          <div className="flex flex-col gap-1.5">
            <label htmlFor="view-name" className="text-xs text-custom-text-300 font-medium">
              视图名称
            </label>
            <input
              id="view-name"
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="输入视图名称..."
              className="border-custom-border-200 bg-custom-background-90 text-sm text-custom-text-100 placeholder:text-custom-text-400 focus:border-custom-primary rounded-md border px-3 py-2 outline-none"
            />
          </div>

          {/* Layout type (read-only) */}
          <div className="flex flex-col gap-1.5">
            <span className="text-xs text-custom-text-300 font-medium">布局类型</span>
            <div className="border-custom-border-200 bg-custom-background-80 text-sm text-custom-text-200 rounded-md border px-3 py-2">
              {LAYOUT_LABELS[currentLayout]}
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="border-custom-border-200 flex items-center justify-end gap-2 border-t px-5 py-4">
          <button
            type="button"
            onClick={handleClose}
            className="border-custom-border-200 text-xs text-custom-text-200 hover:bg-custom-background-80 rounded-md border px-4 py-2 font-medium"
          >
            取消
          </button>
          <button
            type="button"
            onClick={handleSave}
            disabled={!name.trim() || isPending}
            className="bg-custom-primary text-xs hover:bg-custom-primary/90 rounded-md px-4 py-2 font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
          >
            {isPending ? "保存中..." : "保存"}
          </button>
        </div>
      </div>
    </ModalCore>
  );
}
