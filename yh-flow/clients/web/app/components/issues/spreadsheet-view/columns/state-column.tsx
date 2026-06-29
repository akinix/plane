// FLOW: Forked from Plane spreadsheet/columns/state-column.tsx
// Simplified — native select instead of StateDropdown
import { observer } from "mobx-react";
import type { TIssue } from "@plane/types";
import { MOCK_STATES } from "@/../src/lib/mock-data";

type TProps = {
  issue: TIssue;
  onChange?: (issue: TIssue, data: Partial<TIssue>) => void;
  disabled?: boolean;
  onClose?: () => void;
};

export const SpreadsheetStateColumn = observer(function SpreadsheetStateColumn(props: TProps) {
  const { issue, onChange, disabled, onClose } = props;
  const projectStates = MOCK_STATES.filter((s) => s.project_id === issue.project_id).sort((a, b) => a.order - b.order);
  const currentState = projectStates.find((s) => s.id === issue.state_id);

  return (
    <div className="h-11 border-b-[0.5px] border-custom-border-200">
      <select
        value={issue.state_id ?? ""}
        onChange={(e) => {
          if (onChange) onChange(issue, { state_id: e.target.value });
          if (onClose) onClose();
        }}
        disabled={disabled}
        className="h-full w-full appearance-none bg-transparent px-3 text-xs outline-none hover:bg-custom-background-80"
        style={{ color: currentState?.color ?? undefined }}
      >
        {projectStates.map((s) => (
          <option key={s.id} value={s.id}>
            {s.name}
          </option>
        ))}
      </select>
    </div>
  );
});
