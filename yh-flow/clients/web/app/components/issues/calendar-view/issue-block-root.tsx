// FLOW: Forked from Plane calendar/issue-block-root.tsx
// Adapted: replaced @atlaskit/pragmatic-drag-and-drop with @hello-pangea/dnd
// Removed Plane useIssueDetail dependency, use issue from props
import type { TIssue } from "@plane/types";
import { CalendarIssueBlock } from "./issue-block";

type TProps = {
  issue: TIssue;
  index: number;
};

export const CalendarIssueBlockRoot = function CalendarIssueBlockRoot(props: TProps) {
  const { issue, index } = props;

  if (!issue) return null;

  return <CalendarIssueBlock issue={issue} index={index} />;
};
