// FLOW: DeleteIssueModal — soft-delete confirmation dialog (stub for Task 1, full implementation in Task 3)
"use client";

type Props = {
  isOpen: boolean;
  onClose: () => void;
  issueId: string;
  issueName: string;
  projectId: string;
  onDelete: () => void;
};

export const DeleteIssueModal = function DeleteIssueModal(_props: Props) {
  return null;
};
