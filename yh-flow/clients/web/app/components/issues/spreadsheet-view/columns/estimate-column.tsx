// FLOW: Forked from Plane spreadsheet/columns/estimate-column.tsx
// Simplified — native select instead of EstimateDropdown
import { observer } from "mobx-react";
import type { TIssue } from "@plane/types";

type TProps = {
  issue: TIssue;
  onChange?: (issue: TIssue, data: Partial<TIssue>) => void;
  disabled?: boolean;
  onClose?: () => void;
};

const ESTIMATE_OPTIONS = [
  { value: "0", label: "0 points" },
  { value: "1", label: "1 point" },
  { value: "2", label: "2 points" },
  { value: "3", label: "3 points" },
  { value: "5", label: "5 points" },
  { value: "8", label: "8 points" },
  { value: "13", label: "13 points" },
  { value: "21", label: "21 points" },
];

export const SpreadsheetEstimateColumn = observer(function SpreadsheetEstimateColumn(props: TProps) {
  const { issue, onChange, disabled, onClose } = props;

  return (
    <div className="h-11 border-b-[0.5px] border-custom-border-200">
      <select
        value={issue.estimate_point ?? ""}
        onChange={(e) => {
          if (onChange) onChange(issue, { estimate_point: e.target.value || null });
          if (onClose) onClose();
        }}
        disabled={disabled}
        className="h-full w-full appearance-none bg-transparent px-3 text-xs outline-none hover:bg-custom-background-80"
      >
        <option value="">未估算</option>
        {ESTIMATE_OPTIONS.map((opt) => (
          <option key={opt.value} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>
    </div>
  );
});
