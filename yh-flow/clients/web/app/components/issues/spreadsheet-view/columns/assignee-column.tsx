// FLOW: Forked from Plane spreadsheet/columns/assignee-column.tsx
// Simplified — native select instead of MemberDropdown
import { observer } from "mobx-react";
import type { TIssue } from "@plane/types";
import { MOCK_MEMBERS } from "@/../src/lib/mock-data";

type TProps = {
  issue: TIssue;
  onChange?: (issue: TIssue, data: Partial<TIssue>) => void;
  disabled?: boolean;
  onClose?: () => void;
};

export const SpreadsheetAssigneeColumn = observer(function SpreadsheetAssigneeColumn(props: TProps) {
  const { issue, onChange, disabled, onClose } = props;
  const workspaceId = "ws-1"; // mock — use real workspace context in production
  const members = MOCK_MEMBERS[workspaceId] ?? [];
  const selectedId = issue.assignee_ids?.[0] ?? "";

  return (
    <div className="h-11 border-b-[0.5px] border-custom-border-200">
      <select
        value={selectedId}
        onChange={(e) => {
          if (onChange) onChange(issue, { assignee_ids: e.target.value ? [e.target.value] : [] });
          if (onClose) onClose();
        }}
        disabled={disabled}
        className="h-full w-full appearance-none bg-transparent px-3 text-xs outline-none hover:bg-custom-background-80"
      >
        <option value="">未指派</option>
        {members.map((m) => (
          <option key={m.member.id} value={m.member.id}>
            {m.member.display_name}
          </option>
        ))}
      </select>
    </div>
  );
});
