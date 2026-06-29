// FLOW: Forked from Plane spreadsheet/columns/label-column.tsx
// Simplified — native multi-select instead of IssuePropertyLabels
import { observer } from "mobx-react";
import type { TIssue } from "@plane/types";
import { MOCK_LABELS } from "@/../src/lib/mock-data";

type TProps = {
  issue: TIssue;
  onChange?: (issue: TIssue, data: Partial<TIssue>) => void;
  disabled?: boolean;
  onClose?: () => void;
};

export const SpreadsheetLabelColumn = observer(function SpreadsheetLabelColumn(props: TProps) {
  const { issue, onChange, disabled } = props;
  const projectLabels = MOCK_LABELS.filter((l) => l.project_id === issue.project_id);

  const handleToggle = (labelId: string) => {
    if (!onChange) return;
    const current = issue.label_ids ?? [];
    const next = current.includes(labelId)
      ? current.filter((id) => id !== labelId)
      : [...current, labelId];
    onChange(issue, { label_ids: next });
  };

  return (
    <div className="h-11 w-full border-b-[0.5px] border-custom-border-200">
      {disabled ? (
        <div className="flex h-full items-center gap-1 px-3">
          {(issue.label_ids ?? []).length === 0 && <span className="text-xs text-custom-text-400">-</span>}
          {(issue.label_ids ?? []).map((id) => {
            const label = projectLabels.find((l) => l.id === id);
            return label ? (
              <span
                key={id}
                className="inline-block rounded-sm px-1.5 py-0.5 text-xs"
                style={{ backgroundColor: label.color + "20", color: label.color }}
              >
                {label.name}
              </span>
            ) : null;
          })}
        </div>
      ) : (
        <select
          multiple
          value={issue.label_ids ?? []}
          onChange={(e) => {
            const options = Array.from(e.target.selectedOptions, (o) => o.value);
            if (onChange) onChange(issue, { label_ids: options });
          }}
          className="h-full w-full bg-transparent px-3 text-xs outline-none hover:bg-custom-background-80"
        >
          {projectLabels.map((l) => (
            <option key={l.id} value={l.id}>
              {l.name}
            </option>
          ))}
        </select>
      )}
    </div>
  );
});
