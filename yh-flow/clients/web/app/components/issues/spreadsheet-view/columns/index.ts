// FLOW: Forked from Plane spreadsheet/columns/index.ts
// Column registry — maps IIssueDisplayProperties keys to column components
import type { IIssueDisplayProperties, TIssue } from "@plane/types";
import { SpreadsheetAssigneeColumn } from "./assignee-column";
import { SpreadsheetAttachmentColumn } from "./attachment-column";
import { SpreadsheetCreatedOnColumn } from "./created-on-column";
import { SpreadsheetDueDateColumn } from "./due-date-column";
import { SpreadsheetEstimateColumn } from "./estimate-column";
import { SpreadsheetLabelColumn } from "./label-column";
import { SpreadsheetLinkColumn } from "./link-column";
import { SpreadsheetPriorityColumn } from "./priority-column";
import { SpreadsheetStartDateColumn } from "./start-date-column";
import { SpreadsheetStateColumn } from "./state-column";
import { SpreadsheetSubIssueColumn } from "./sub-issue-column";
import { SpreadsheetUpdatedOnColumn } from "./updated-on-column";
import { SpreadsheetModuleColumn } from "./module-column";
import { SpreadsheetCycleColumn } from "./cycle-column";
import { HeaderColumn } from "./header-column";

type TSpreadsheetColumn = React.ComponentType<{
  issue: TIssue;
  onChange?: (issue: TIssue, data: Partial<TIssue>) => void;
  disabled?: boolean;
  onClose?: () => void;
}>;

export const SPREADSHEET_COLUMNS: Partial<Record<keyof IIssueDisplayProperties, TSpreadsheetColumn>> = {
  assignee: SpreadsheetAssigneeColumn,
  created_on: SpreadsheetCreatedOnColumn,
  due_date: SpreadsheetDueDateColumn,
  estimate: SpreadsheetEstimateColumn,
  labels: SpreadsheetLabelColumn,
  modules: SpreadsheetModuleColumn,
  cycle: SpreadsheetCycleColumn,
  link: SpreadsheetLinkColumn,
  priority: SpreadsheetPriorityColumn,
  start_date: SpreadsheetStartDateColumn,
  state: SpreadsheetStateColumn,
  sub_issue_count: SpreadsheetSubIssueColumn,
  updated_on: SpreadsheetUpdatedOnColumn,
  attachment_count: SpreadsheetAttachmentColumn,
};
