// FLOW: Forked from Plane spreadsheet/spreadsheet-header.tsx
// Simplified — uses store for select-all, removes MultipleSelectGroupAction
import { observer } from "mobx-react";
import type { IIssueDisplayFilterOptions, IIssueDisplayProperties } from "@plane/types";
import { useStore } from "@/lib/store-context";
import { SpreadsheetHeaderColumn } from "./spreadsheet-header-column";

type TProps = {
  displayProperties: IIssueDisplayProperties;
  displayFilters: IIssueDisplayFilterOptions;
  handleDisplayFilterUpdate: (data: Partial<IIssueDisplayFilterOptions>) => void;
  isEstimateEnabled: boolean;
  spreadsheetColumnsList: (keyof IIssueDisplayProperties)[];
  issueIds: string[];
};

export const SpreadsheetHeader = observer(function SpreadsheetHeader(props: TProps) {
  const {
    displayProperties,
    displayFilters,
    handleDisplayFilterUpdate,
    isEstimateEnabled,
    spreadsheetColumnsList,
    issueIds,
  } = props;
  const store = useStore();
  const isAllSelected = issueIds.length > 0 && store.issue.selectedIssueIds.length === issueIds.length;

  const handleSelectAll = () => {
    if (isAllSelected) {
      store.issue.clearSelection();
    } else {
      store.issue.selectAll(issueIds);
    }
  };

  return (
    <thead className="sticky top-0 left-0 z-[12] border-b-[0.5px] border-custom-border-200">
      <tr>
        {/* Sticky header column with select-all */}
        <th className="sticky left-0 z-[15] h-11 min-w-[200px] border-r-[0.5px] border-custom-border-200 bg-custom-background-90 text-left text-13 font-medium">
          <div className="flex h-full items-center gap-2 px-3">
            <input
              type="checkbox"
              checked={isAllSelected}
              onChange={handleSelectAll}
              className="size-3.5 rounded border-custom-border-200 text-custom-primary"
            />
            <span className="text-xs font-medium text-custom-text-200">Work items</span>
          </div>
        </th>

        {spreadsheetColumnsList.map((property) => (
          <SpreadsheetHeaderColumn
            key={property}
            property={property}
            displayProperties={displayProperties}
            displayFilters={displayFilters}
            handleDisplayFilterUpdate={handleDisplayFilterUpdate}
            isEstimateEnabled={isEstimateEnabled}
          />
        ))}
      </tr>
    </thead>
  );
});
