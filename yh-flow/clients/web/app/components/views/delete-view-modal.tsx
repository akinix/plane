// FLOW: Forked from Plane. Original: apps/web/core/components/views/delete-view-modal.tsx
// FLOW: DeleteViewModal — 删除视图确认弹窗
"use client";
import { useState } from "react";
import { observer } from "mobx-react";
import { AlertModalCore } from "@plane/ui";
import { useViewMutations } from "@/../src/lib/hooks/use-view-mutations";

type Props = {
  isOpen: boolean;
  onClose: () => void;
  viewId: string;
  viewName: string;
  projectId: string;
};

const DeleteViewModal = observer(function DeleteViewModal({ isOpen, onClose, viewId, viewName: _viewName, projectId: _projectId }: Props) {
  const [isDeleting, setIsDeleting] = useState(false);
  const { deleteView } = useViewMutations();

  const handleDelete = async () => {
    setIsDeleting(true);
    try {
      await deleteView.mutateAsync(viewId);
      onClose();
    } catch {
      // Error handled by mutation
    }
    setIsDeleting(false);
  };

  return (
    <AlertModalCore
      handleClose={onClose}
      handleSubmit={handleDelete}
      isSubmitting={isDeleting}
      isOpen={isOpen}
      title="删除视图"
      content={<p className="text-sm text-custom-text-300">确定删除此视图？此操作不可撤销。</p>}
    />
  );
});

export { DeleteViewModal };
