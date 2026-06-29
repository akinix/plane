// FLOW: Forked from Plane spreadsheet/issue-column.tsx
// Simplified — uses local columns registry instead of SPREADSHEET_COLUMNS from plane-web
import { useRef } from "react";
import { observer } from "mobx-react";
import type { IIssueDisplayProperties, TIssue } from "@plane/types";
import { SPREADSHEET_COLUMNS } from "./columns";

type TProps = {
  displayProperties: IIssueDisplayProperties;
  issueDetail: TIssue;
  disableUserActions: boolean;
  property: keyof IIssueDisplayProperties;
  updateIssue: (projectId: string | null, issueId: string, data: Partial<TIssue>) => Promise<void> | undefined;
  isEstimateEnabled?: boolean;
};

export const IssueColumn = observer(function IssueColumn(props: TProps) {
  const { issueDetail, disableUserActions, property, updateIssue } = props;
  const tableCellRef = useRef<HTMLTableCellElement | null>(null);

  const Column = SPREADSHEET_COLUMNS[property];
  if (!Column) return null;

  const handleUpdateIssue = async (issue: TIssue, data: Partial<TIssue>) => {
    if (updateIssue) await updateIssue(issue.project_id, issue.id, data);
  };

  return (
    <td
      tabIndex={0}
      className="h-11 min-w-36 border-r-[1px] border-custom-border-200 text-13 after:absolute after:bottom-[-1px] after:w-full after:border after:border-custom-border-200"
      ref={tableCellRef}
    >
      <Column
        issue={issueDetail}
        onChange={handleUpdateIssue}
        disabled={disableUserActions}
        onClose={() => tableCellRef?.current?.focus()}
      />
    </td>
  );
});
