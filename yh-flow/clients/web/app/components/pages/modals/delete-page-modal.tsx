// FLOW: Forked from Plane. Original: apps/web/core/components/pages/modals/delete-page-modal.tsx
// FLOW: DeletePageModal — 删除页面确认弹窗
"use client";
import { useState } from "react";
import { usePageMutations } from "@/../src/lib/hooks/use-page-mutations";

type Props = {
  pageId: string;
  pageName: string;
  isOpen: boolean;
  onClose: () => void;
  workspaceId: string;
};

export const DeletePageModal = function DeletePageModal({
  pageId,
  pageName,
  isOpen,
  onClose,
  workspaceId: _workspaceId,
}: Props) {
  const [isDeleting, setIsDeleting] = useState(false);
  const { deletePage } = usePageMutations();

  const handleDelete = async () => {
    if (!pageId) return;
    setIsDeleting(true);
    try {
      await deletePage.mutateAsync(pageId);
      onClose();
    } catch (error) {
      console.error("Failed to delete page:", error);
    } finally {
      setIsDeleting(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <button className="fixed inset-0 cursor-default bg-black/50" onClick={onClose} type="button" aria-label="关闭" />
      <div className="border-custom-border-200 bg-custom-background-100 shadow-xl relative z-10 w-full max-w-md rounded-lg border p-6">
        <h3 className="text-lg text-custom-text-100 font-medium">删除页面</h3>
        <p className="text-sm text-custom-text-200 mt-2">
          确定删除此页面？
          <span className="text-custom-text-100 mt-1 block font-medium">{pageName}</span>
        </p>
        <p className="text-xs text-custom-text-300 mt-1">此操作不可撤销。</p>
        <div className="mt-6 flex items-center justify-end gap-2">
          <button
            onClick={onClose}
            className="border-custom-border-200 text-sm text-custom-text-100 hover:bg-custom-background-80 rounded-md border px-4 py-2"
          >
            取消
          </button>
          <button
            onClick={handleDelete}
            disabled={isDeleting}
            className="bg-red-500 text-sm hover:bg-red-600 rounded-md px-4 py-2 text-white disabled:opacity-50"
          >
            {isDeleting ? "删除中..." : "删除"}
          </button>
        </div>
      </div>
    </div>
  );
};
