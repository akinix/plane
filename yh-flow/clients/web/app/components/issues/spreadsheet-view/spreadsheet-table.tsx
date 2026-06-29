// FLOW: Forked from Plane spreadsheet/spreadsheet-table.tsx
// Simplified — no virtual scrolling, no infinite load, scroll shadow effect retained
import { useCallback, useEffect, useRef } from "react";
import { observer } from "mobx-react";
import type { IIssueDisplayFilterOptions, IIssueDisplayProperties, TIssue } from "@plane/types";
import { SpreadsheetIssueRow } from "./issue-row";
import { SpreadsheetHeader } from "./spreadsheet-header";

type TProps = {
  displayProperties: IIssueDisplayProperties;
  displayFilters: IIssueDisplayFilterOptions;
  handleDisplayFilterUpdate: (data: Partial<IIssueDisplayFilterOptions>) => void;
  issues: TIssue[];
  isEstimateEnabled: boolean;
  updateIssue: (projectId: string | null, issueId: string, data: Partial<TIssue>) => Promise<void> | undefined;
  spreadsheetColumnsList: (keyof IIssueDisplayProperties)[];
  containerRef: React.RefObject<HTMLDivElement | null>;
};

export const SpreadsheetTable = observer(function SpreadsheetTable(props: TProps) {
  const {
    displayProperties,
    displayFilters,
    handleDisplayFilterUpdate,
    issues,
    isEstimateEnabled,
    updateIssue,
    spreadsheetColumnsList,
    containerRef,
  } = props;

  const isScrolled = useRef(false);

  const handleScroll = useCallback(() => {
    if (!containerRef.current) return;
    const scrollLeft = containerRef.current.scrollLeft;
    const columnShadow = "8px 22px 22px 10px rgba(0, 0, 0, 0.05)";
    const headerShadow = "8px -22px 22px 10px rgba(0, 0, 0, 0.05)";

    if (scrollLeft > 0 !== isScrolled.current) {
      const firstColumns = containerRef.current.querySelectorAll("table tr td:first-child, th:first-child");
      for (let i = 0; i < firstColumns.length; i++) {
        const shadow = i === 0 ? headerShadow : columnShadow;
        (firstColumns[i] as HTMLElement).style.boxShadow = scrollLeft > 0 ? shadow : "none";
      }
      isScrolled.current = scrollLeft > 0;
    }
  }, [containerRef]);

  useEffect(() => {
    const el = containerRef.current;
    if (el) el.addEventListener("scroll", handleScroll);
    return () => {
      if (el) el.removeEventListener("scroll", handleScroll);
    };
  }, [handleScroll, containerRef]);

  return (
    <table className="w-full overflow-y-auto bg-custom-background-100">
      <SpreadsheetHeader
        displayProperties={displayProperties}
        displayFilters={displayFilters}
        handleDisplayFilterUpdate={handleDisplayFilterUpdate}
        isEstimateEnabled={isEstimateEnabled}
        spreadsheetColumnsList={spreadsheetColumnsList}
        issueIds={issues.map((i) => i.id)}
      />
      <tbody>
        {issues.map((issue) => (
          <SpreadsheetIssueRow
            key={issue.id}
            issueId={issue.id}
            issue={issue}
            displayProperties={displayProperties}
            spreadsheetColumnsList={spreadsheetColumnsList}
            updateIssue={updateIssue}
          />
        ))}
      </tbody>
    </table>
  );
});
