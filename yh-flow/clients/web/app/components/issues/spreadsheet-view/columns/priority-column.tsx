// FLOW: Forked from Plane spreadsheet/columns/priority-column.tsx
// Simplified — native select instead of PriorityDropdown
import { observer } from "mobx-react";
import type { TIssue } from "@plane/types";

type TProps = {
  issue: TIssue;
  onChange?: (issue: TIssue, data: Partial<TIssue>) => void;
  disabled?: boolean;
  onClose?: () => void;
};

const PRIORITY_OPTIONS = [
  { value: "urgent", label: "紧急", color: "#D1453B" },
  { value: "high", label: "高", color: "#D97706" },
  { value: "medium", label: "中", color: "#EAB308" },
  { value: "low", label: "低", color: "#3B82F6" },
  { value: "none", label: "无", color: "#9CA3AF" },
];

export const SpreadsheetPriorityColumn = observer(function SpreadsheetPriorityColumn(props: TProps) {
  const { issue, onChange, disabled, onClose } = props;
  const current = PRIORITY_OPTIONS.find((p) => p.value === (issue.priority ?? "none"));

  return (
    <div className="h-11 border-b-[0.5px] border-custom-border-200">
      <select
        value={issue.priority ?? "none"}
        onChange={(e) => {
          if (onChange) onChange(issue, { priority: e.target.value as TIssue["priority"] });
          if (onClose) onClose();
        }}
        disabled={disabled}
        className="h-full w-full appearance-none bg-transparent px-3 text-xs outline-none hover:bg-custom-background-80"
        style={{ color: current?.color ?? undefined }}
      >
        {PRIORITY_OPTIONS.map((p) => (
          <option key={p.value} value={p.value}>
            {p.label}
          </option>
        ))}
      </select>
    </div>
  );
});
