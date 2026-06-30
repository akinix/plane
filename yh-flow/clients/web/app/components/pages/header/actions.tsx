// FLOW: Forked from Plane. Original: apps/web/core/components/pages/header/actions.tsx
// FLOW: PageHeaderActions — 页面头部操作按钮（归档/删除）
"use client";
import { useState } from "react";
import { Archive, Trash2 } from "lucide-react";
import type { TPage } from "@plane/types";
import { usePageMutations } from "@/../src/lib/hooks/use-page-mutations";
import { DeletePageModal } from "@/components/pages/modals/delete-page-modal";

type Props = {
  page: TPage;
  workspaceId: string;
};

export const PageHeaderActions = function PageHeaderActions({ page, workspaceId }: Props) {
  const [showArchiveConfirm, setShowArchiveConfirm] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const { archivePage } = usePageMutations();

  const handleArchive = async () => {
    try {
      await archivePage.mutateAsync(page.id);
      setShowArchiveConfirm(false);
    } catch {
      // Archive failed — confirmation dialog stays open for retry
    }
  };

  return (
    <>
      <div className="flex items-center gap-1">
        {/* Archive button */}
        <button
          type="button"
          onClick={() => setShowArchiveConfirm(true)}
          className="text-xs text-custom-text-300 hover:bg-custom-background-80 flex items-center gap-1.5 rounded px-2 py-1 transition-colors"
        >
          <Archive className="size-3.5" />
          归档
        </button>

        {/* Delete button */}
        <button
          type="button"
          onClick={() => setShowDeleteModal(true)}
          className="text-xs text-red-500 hover:bg-custom-background-80 flex items-center gap-1.5 rounded px-2 py-1 transition-colors"
        >
          <Trash2 className="size-3.5" />
          删除
        </button>
      </div>

      {/* Archive confirmation dialog */}
      {showArchiveConfirm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
          <button
            type="button"
            className="fixed inset-0 cursor-default bg-black/50"
            onClick={() => setShowArchiveConfirm(false)}
            aria-label="关闭"
          />
          <div className="border-custom-border-200 bg-custom-background-100 shadow-xl relative z-10 w-full max-w-md rounded-lg border p-6">
            <h3 className="text-lg text-custom-text-100 font-medium">归档页面</h3>
            <p className="text-sm text-custom-text-200 mt-2">确定归档此页面？归档后仅归档选项卡中可见。</p>
            <div className="mt-6 flex items-center justify-end gap-2">
              <button
                type="button"
                onClick={() => setShowArchiveConfirm(false)}
                className="border-custom-border-200 text-sm text-custom-text-100 hover:bg-custom-background-80 rounded-md border px-4 py-2"
              >
                取消
              </button>
              <button
                type="button"
                onClick={handleArchive}
                className="bg-custom-primary text-sm rounded-md px-4 py-2 text-white hover:opacity-90"
              >
                归档
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Delete modal */}
      <DeletePageModal
        pageId={page.id}
        pageName={page.name}
        isOpen={showDeleteModal}
        onClose={() => setShowDeleteModal(false)}
        workspaceId={workspaceId}
      />
    </>
  );
};
