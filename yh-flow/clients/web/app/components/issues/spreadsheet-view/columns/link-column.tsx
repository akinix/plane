// FLOW: Forked from Plane spreadsheet/columns/link-column.tsx
// Read-only — displays link count
import { observer } from "mobx-react";
import type { TIssue } from "@plane/types";

type TProps = {
  issue: TIssue;
  onChange?: (issue: TIssue, data: Partial<TIssue>) => void;
  disabled?: boolean;
  onClose?: () => void;
};

export const SpreadsheetLinkColumn = observer(function SpreadsheetLinkColumn(props: TProps) {
  const { issue } = props;
  const count = issue?.link_count ?? 0;

  return (
    <div className="flex h-11 w-full items-center border-b-[0.5px] border-custom-border-200 px-3 text-xs text-custom-text-400">
      {count} {count === 1 ? "link" : "links"}
    </div>
  );
});
