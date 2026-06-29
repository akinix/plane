// FLOW: Forked from Plane spreadsheet/issue-row.tsx
// Simplified for yh-flow — no RenderIfVisible, no sub-issue tree, direct store selection
import { observer } from "mobx-react";
import type { IIssueDisplayProperties, TIssue } from "@plane/types";
import { cn } from "@plane/utils";
import { useStore } from "@/lib/store-context";
import { IssueColumn } from "./issue-column";

type TProps = {
  displayProperties: IIssueDisplayProperties;
  issueId: string;
  issue: TIssue;
  spreadsheetColumnsList: (keyof IIssueDisplayProperties)[];
  updateIssue: (projectId: string | null, issueId: string, data: Partial<TIssue>) => Promise<void> | undefined;
};

export const SpreadsheetIssueRow = observer(function SpreadsheetIssueRow(props: TProps) {
  const { displayProperties, issueId, issue, spreadsheetColumnsList, updateIssue } = props;
  const store = useStore();
  const isIssueSelected = store.issue.selectedIssueIds.includes(issueId);
  const disableUserActions = false; // placeholder — no permissions check in mock mode

  return (
    <tr
      className={cn("border-b-[0.5px] border-custom-border-200 bg-custom-background-100 transition-colors hover:bg-custom-background-90", {
        "bg-custom-primary/5": isIssueSelected,
      })}
    >
      {/* Checkbox + identifier + name — sticky first column */}
      <td className="sticky left-0 z-10 border-r-[0.5px] border-custom-border-200 bg-custom-background-100 md:sticky">
        <div className="flex h-11 items-center gap-2 px-3">
          <input
            type="checkbox"
            checked={isIssueSelected}
            onChange={() => store.issue.toggleIssueSelection(issueId)}
            className="size-3.5 rounded border-custom-border-200 text-custom-primary"
          />
          <span className="min-w-[60px] text-xs font-medium text-custom-text-400">
            {issue.sequence_id}
          </span>
          <span className="truncate text-sm text-custom-text-100">
            {issue.name}
          </span>
        </div>
      </td>

      {/* Data columns */}
      {spreadsheetColumnsList.map((property) => (
        <IssueColumn
          key={property}
          displayProperties={displayProperties}
          issueDetail={issue}
          disableUserActions={disableUserActions}
          property={property}
          updateIssue={updateIssue}
        />
      ))}
    </tr>
  );
});
