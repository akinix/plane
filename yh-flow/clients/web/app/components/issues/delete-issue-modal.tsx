// FLOW: DeleteIssueModal — soft-delete confirmation dialog per ISSUE-05
"use client";

import { useState } from "react";
import { AlertModalCore } from "@plane/ui";
import { EModalWidth } from "@plane/ui";
import { useIssueMutations } from "@/../src/lib/hooks/use-issues";

type Props = {
  isOpen: boolean;
  onClose: () => void;
  issueId: string;
  issueName: string;
  projectId: string;
  onDelete: () => void;
};

export const DeleteIssueModal = function DeleteIssueModal({ isOpen, onClose, issueId, issueName, onDelete }: Props) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { deleteIssue } = useIssueMutations();

  const handleSubmit = async () => {
    setIsSubmitting(true);
    try {
      await deleteIssue.mutateAsync({ issueId });
      onDelete();
      onClose();
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <AlertModalCore
      isOpen={isOpen}
      handleClose={onClose}
      handleSubmit={handleSubmit}
      isSubmitting={isSubmitting}
      title="删除 Issue"
      content={
        <div>
          <p>确定删除此 Issue 吗？此操作不可撤销。</p>
          <p className="text-custom-text-100 mt-2 font-medium">{issueName}</p>
        </div>
      }
      variant="danger"
      width={EModalWidth.XL}
      primaryButtonText={{
        loading: "删除中...",
        default: "删除",
      }}
      secondaryButtonText="取消"
    />
  );
};
