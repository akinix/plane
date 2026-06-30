// FLOW: Forked from Plane. Original: apps/web/core/components/views/quick-actions.tsx
// FLOW: ViewQuickActions — 视图快捷操作下拉（删除、复制链接）
"use client";
import { useState } from "react";
import { observer } from "mobx-react";
import { MoreHorizontal, Copy, Trash2 } from "lucide-react";
import type { TIssueView } from "@/components/issues/filters/types";
import { copyUrlToClipboard } from "@plane/utils";
import { TOAST_TYPE, setToast } from "@plane/propel/toast";

type Props = {
  view: TIssueView;
  projectId: string;
  workspaceSlug: string;
  onEdit?: () => void;
  onDelete?: () => void;
};

const ViewQuickActions = observer(function ViewQuickActions({ view, projectId, workspaceSlug, onEdit: _onEdit, onDelete }: Props) {
  const [menuOpen, setMenuOpen] = useState(false);

  const handleCopyLink = () => {
    const link = `${workspaceSlug}/projects/${projectId}/views/${view.id}`;
    copyUrlToClipboard(link).then(() => {
      setToast({
        type: TOAST_TYPE.SUCCESS,
        title: "已复制",
        message: "视图链接已复制到剪贴板",
      });
      return undefined;
    });
    setMenuOpen(false);
  };

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() => setMenuOpen(!menuOpen)}
        className="text-custom-text-400 hover:text-custom-text-200 rounded p-1"
      >
        <MoreHorizontal className="size-4" />
      </button>

      {menuOpen && (
        <div
          className="shadow-custom-shadow-2xs border-custom-border-200 bg-custom-background-90 absolute right-0 top-full z-10 mt-1 min-w-[140px] rounded-md border p-1"
          onMouseLeave={() => setMenuOpen(false)}
        >
          <button
            type="button"
            onClick={handleCopyLink}
            className="hover:bg-custom-background-80 text-custom-text-200 hover:text-custom-text-100 flex w-full items-center gap-2 rounded-sm px-2 py-1.5 text-xs"
          >
            <Copy className="size-3.5" />
            复制链接
          </button>
          {onDelete && (
            <button
              type="button"
              onClick={() => {
                setMenuOpen(false);
                onDelete();
              }}
              className="hover:bg-custom-background-80 text-red-500 flex w-full items-center gap-2 rounded-sm px-2 py-1.5 text-xs"
            >
              <Trash2 className="size-3.5" />
              删除
            </button>
          )}
        </div>
      )}
    </div>
  );
});

export { ViewQuickActions };
