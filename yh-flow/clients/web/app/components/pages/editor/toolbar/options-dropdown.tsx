// FLOW: Forked from Plane. Original: apps/web/core/components/pages/editor/toolbar/options-dropdown.tsx
// FLOW: PageOptionsDropdown — 更多选项下拉（预留扩展）
"use client";
import { useState } from "react";
import { Maximize2, FileText } from "lucide-react";
import { CustomMenu } from "@plane/ui";

type Props = {
  isFullWidth?: boolean;
  onToggleFullWidth?: () => void;
};

export const PageOptionsDropdown = function PageOptionsDropdown({ isFullWidth, onToggleFullWidth }: Props) {
  const [_, _set] = useState(false);

  return (
    <CustomMenu
      customButton={
        <button
          type="button"
          className="text-custom-text-400 hover:bg-custom-background-80 grid size-7 place-items-center rounded-sm transition-colors"
          aria-label="更多选项"
        >
          <Maximize2 className="size-3.5" />
        </button>
      }
      placement="bottom-end"
    >
      <CustomMenu.MenuItem onClick={() => onToggleFullWidth?.()}>
        <Maximize2 className="size-3.5" />
        {isFullWidth ? "标准宽度" : "全屏宽度"}
      </CustomMenu.MenuItem>
      <CustomMenu.MenuItem>
        <FileText className="size-3.5" />
        字数统计
      </CustomMenu.MenuItem>
    </CustomMenu>
  );
};
