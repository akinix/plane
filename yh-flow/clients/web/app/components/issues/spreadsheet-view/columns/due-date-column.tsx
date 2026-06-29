// FLOW: Forked from Plane spreadsheet/columns/due-date-column.tsx
// Simplified — native date input instead of DateDropdown
import { observer } from "mobx-react";
import type { TIssue } from "@plane/types";

type TProps = {
  issue: TIssue;
  onChange?: (issue: TIssue, data: Partial<TIssue>) => void;
  disabled?: boolean;
  onClose?: () => void;
};

export const SpreadsheetDueDateColumn = observer(function SpreadsheetDueDateColumn(props: TProps) {
  const { issue, onChange, disabled } = props;

  const dateValue = issue.target_date ? issue.target_date.split("T")[0] : "";

  return (
    <div className="h-11 border-b-[0.5px] border-custom-border-200">
      {disabled ? (
        <div className="flex h-full items-center px-3 text-xs text-custom-text-400">
          {issue.target_date ? new Date(issue.target_date).toLocaleDateString("zh-CN") : "-"}
        </div>
      ) : (
        <input
          type="date"
          value={dateValue}
          onChange={(e) => {
            if (onChange) {
              const val = e.target.value;
              onChange(issue, { target_date: val ? new Date(val).toISOString() : null });
            }
          }}
          className="h-full w-full bg-transparent px-3 text-xs outline-none hover:bg-custom-background-80"
        />
      )}
    </div>
  );
});
