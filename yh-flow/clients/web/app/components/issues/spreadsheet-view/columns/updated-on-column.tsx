// FLOW: Forked from Plane spreadsheet/columns/updated-on-column.tsx
// Read-only — displays updated_at date
import { observer } from "mobx-react";
import type { TIssue } from "@plane/types";

type TProps = {
  issue: TIssue;
  onChange?: (issue: TIssue, data: Partial<TIssue>) => void;
  disabled?: boolean;
  onClose?: () => void;
};

export const SpreadsheetUpdatedOnColumn = observer(function SpreadsheetUpdatedOnColumn(props: TProps) {
  const { issue } = props;

  return (
    <div className="flex h-11 w-full items-center border-b-[0.5px] border-custom-border-200 px-3 text-xs text-custom-text-400">
      {issue.updated_at ? new Date(issue.updated_at).toLocaleDateString("zh-CN") : "-"}
    </div>
  );
});
