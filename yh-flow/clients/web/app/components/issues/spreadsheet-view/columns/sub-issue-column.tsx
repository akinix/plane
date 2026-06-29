// FLOW: Forked from Plane spreadsheet/columns/sub-issue-column.tsx
// Read-only — displays sub-issue count
import { observer } from "mobx-react";
import type { TIssue } from "@plane/types";

type TProps = {
  issue: TIssue;
  onChange?: (issue: TIssue, data: Partial<TIssue>) => void;
  disabled?: boolean;
  onClose?: () => void;
};

export const SpreadsheetSubIssueColumn = observer(function SpreadsheetSubIssueColumn(props: TProps) {
  const { issue } = props;
  const count = issue?.sub_issues_count ?? 0;

  return (
    <div className="flex h-11 w-full items-center border-b-[0.5px] border-custom-border-200 px-3 text-xs text-custom-text-400">
      {count > 0 ? `${count} sub-work item${count !== 1 ? "s" : ""}` : "-"}
    </div>
  );
});
