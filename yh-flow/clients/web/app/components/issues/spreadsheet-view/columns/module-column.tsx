// FLOW: Forked from Plane spreadsheet/columns/module-column.tsx
// Simplified — native select instead of ModuleDropdown
import { observer } from "mobx-react";
import type { TIssue } from "@plane/types";

type TProps = {
  issue: TIssue;
  onChange?: (issue: TIssue, data: Partial<TIssue>) => void;
  disabled?: boolean;
  onClose?: () => void;
};

// Placeholder modules — real data from API in production
const MOCK_MODULES = [
  { id: "mod-1", name: "前端" },
  { id: "mod-2", name: "后端" },
  { id: "mod-3", name: "基础设施" },
];

export const SpreadsheetModuleColumn = observer(function SpreadsheetModuleColumn(props: TProps) {
  const { issue, onChange, disabled } = props;
  const selectedIds = issue.module_ids ?? [];

  return (
    <div className="h-11 border-b-[0.5px] border-custom-border-200">
      {disabled ? (
        <div className="flex h-full items-center px-3 text-xs text-custom-text-400">
          {selectedIds.length === 0 ? "-" : selectedIds.length + " 个模块"}
        </div>
      ) : (
        <select
          multiple
          value={selectedIds}
          onChange={(e) => {
            const options = Array.from(e.target.selectedOptions, (o) => o.value);
            if (onChange) onChange(issue, { module_ids: options });
          }}
          className="h-full w-full bg-transparent px-3 text-xs outline-none hover:bg-custom-background-80"
        >
          {MOCK_MODULES.map((m) => (
            <option key={m.id} value={m.id}>
              {m.name}
            </option>
          ))}
        </select>
      )}
    </div>
  );
});
