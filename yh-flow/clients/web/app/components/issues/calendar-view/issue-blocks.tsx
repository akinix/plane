// FLOW: Forked from Plane calendar/issue-blocks.tsx
// Simplified: removed Plane store hooks, pagination, quick-add, load-more
import type { TIssue } from "@plane/types";
import { CalendarIssueBlock } from "./issue-block";

type TProps = {
  date: Date;
  formattedDatePayload: string;
  handleDragAndDrop: (
    issueId: string | undefined,
    issueProjectId: string | undefined,
    sourceDate: string | undefined,
    destinationDate: string | undefined
  ) => Promise<void>;
  /** Issues belonging to this date, sorted by target_date for span display */
  issues?: TIssue[];
};

export const CalendarIssueBlocks = function CalendarIssueBlocks(props: TProps) {
  const { issues } = props;

  if (!issues || issues.length === 0) return null;

  return (
    <>
      {issues.map((issue, idx) => (
        <CalendarIssueBlock key={issue.id} issue={issue} index={idx} />
      ))}
    </>
  );
};
