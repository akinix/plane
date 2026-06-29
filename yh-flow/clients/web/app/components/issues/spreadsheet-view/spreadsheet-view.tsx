// FLOW: Forked from Plane spreadsheet/spreadsheet-view.tsx
// Simplified for yh-flow — standalone container with TanStack Query + MobX store
import { useRef, useMemo, useCallback, useEffect } from "react";
import { observer } from "mobx-react";
import type { IIssueDisplayProperties, TIssue } from "@plane/types";
import { SPREADSHEET_PROPERTY_LIST } from "@plane/constants";
import { useStore } from "@/lib/store-context";
import { useIssues, useIssueMutations } from "@/../src/lib/hooks/use-issues";
import { SpreadsheetTable } from "./spreadsheet-table";
import { BulkActionBar } from "../bulk-action-bar";

type TProps = {
  workspaceId: string;
  projectId: string;
  issueIds: string[];
  issues: TIssue[];
};

const DEFAULT_DISPLAY_PROPERTIES: IIssueDisplayProperties = {
  state: true,
  priority: true,
  assignee: true,
  labels: true,
  start_date: true,
  due_date: true,
  estimate: true,
  created_on: true,
  updated_on: true,
  link: true,
  attachment_count: true,
  sub_issue_count: true,
  modules: true,
  cycle: true,
  key: true,
};

const DEFAULT_DISPLAY_FILTERS = {
  order_by: "-created_at",
};

export const SpreadsheetView = observer(function SpreadsheetView({ workspaceId, projectId, issueIds, issues }: TProps) {
  const store = useStore();
  const { updateIssue } = useIssueMutations();
  const containerRef = useRef<HTMLDivElement | null>(null);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  // Persist visible columns from store
  const visibleColumnIds = store.issue.visibleColumnIds;

  // Determine which spreadsheet columns to render
  const spreadsheetColumnsList = useMemo(() => {
    if (visibleColumnIds.length > 0) {
      return SPREADSHEET_PROPERTY_LIST.filter((col) => visibleColumnIds.includes(col));
    }
    return SPREADSHEET_PROPERTY_LIST;
  }, [visibleColumnIds]);

  // Inline edit handler with 800ms debounce (per UI-SPEC and T-17-SHE-01)
  const handleUpdateIssue = useCallback(
    (projectId: string | null, issueId: string, data: Partial<TIssue>) => {
      return new Promise<void>((resolve) => {
        if (debounceRef.current) clearTimeout(debounceRef.current);
        debounceRef.current = setTimeout(() => {
          updateIssue.mutate({ issueId, data });
          resolve();
        }, 800);
      });
    },
    [updateIssue]
  );

  // Cleanup debounce on unmount
  useEffect(() => {
    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
    };
  }, []);

  // We should show the BulkActionBar based on selection
  // (BulkActionBar reads selectedIssueIds internally)

  return (
    <div className="relative flex h-full w-full flex-col overflow-x-hidden bg-custom-background-90 whitespace-nowrap text-custom-text-200">
      <div ref={containerRef} className="vertical-scrollbar horizontal-scrollbar scrollbar-lg h-full w-full overflow-auto">
        <SpreadsheetTable
          displayProperties={DEFAULT_DISPLAY_PROPERTIES}
          displayFilters={DEFAULT_DISPLAY_FILTERS as any}
          handleDisplayFilterUpdate={() => {}}
          issues={issues}
          isEstimateEnabled={false}
          updateIssue={handleUpdateIssue}
          spreadsheetColumnsList={spreadsheetColumnsList}
          containerRef={containerRef}
        />
      </div>

      {/* Bulk action bar — rendered when items are selected */}
      <BulkActionBar
        workspaceId={workspaceId}
        projectId={projectId}
        selectedIds={store.issue.selectedIssueIds}
        onClearSelection={() => store.issue.clearSelection()}
      />
    </div>
  );
});
