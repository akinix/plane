// FLOW: Forked from Plane spreadsheet/columns/cycle-column.tsx
// Simplified — native select instead of CycleDropdown
import { observer } from "mobx-react";
import type { TIssue } from "@plane/types";
import { MOCK_PROJECTS } from "@/../src/lib/mock-data";

type TProps = {
  issue: TIssue;
  onChange?: (issue: TIssue, data: Partial<TIssue>) => void;
  disabled?: boolean;
  onClose?: () => void;
};

// Placeholder cycles — real data from API in production
const MOCK_CYCLES = [
  { id: "cycle-1", name: "Sprint 1" },
  { id: "cycle-2", name: "Sprint 2" },
  { id: "cycle-3", name: "Sprint 3" },
];

export const SpreadsheetCycleColumn = observer(function SpreadsheetCycleColumn(props: TProps) {
  const { issue, onChange, disabled, onClose } = props;

  return (
    <div className="h-11 border-b-[0.5px] border-custom-border-200">
      <select
        value={issue.cycle_id ?? ""}
        onChange={(e) => {
          if (onChange) onChange(issue, { cycle_id: e.target.value || null });
          if (onClose) onClose();
        }}
        disabled={disabled}
        className="h-full w-full appearance-none bg-transparent px-3 text-xs outline-none hover:bg-custom-background-80"
      >
        <option value="">无周期</option>
        {MOCK_CYCLES.map((c) => (
          <option key={c.id} value={c.id}>
            {c.name}
          </option>
        ))}
      </select>
    </div>
  );
});
