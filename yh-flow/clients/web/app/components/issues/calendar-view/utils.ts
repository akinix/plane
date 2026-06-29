// FLOW: Forked from Plane calendar/utils.ts
// Adapted: use simpler updateIssue interface (mutation-based instead of direct API call)
import type { TIssue } from "@plane/types";

export type UpdateIssueFn = (issueId: string, data: Partial<TIssue>) => void;

export const handleDragDrop = (
  issueId: string,
  sourceDate: string,
  destinationDate: string,
  updateIssue?: UpdateIssueFn
): void => {
  if (!updateIssue) return;
  if (sourceDate === destinationDate) return;

  updateIssue(issueId, {
    target_date: destinationDate,
  });
};
