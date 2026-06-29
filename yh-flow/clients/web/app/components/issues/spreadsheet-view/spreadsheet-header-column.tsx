// FLOW: Forked from Plane spreadsheet/spreadsheet-header-column.tsx
// Simplified — no CustomMenu, no SPREADSHEET_PROPERTY_DETAILS, plain column name
import { observer } from "mobx-react";
import type { IIssueDisplayFilterOptions, IIssueDisplayProperties } from "@plane/types";

type TProps = {
  displayProperties: IIssueDisplayProperties;
  property: keyof IIssueDisplayProperties;
  isEstimateEnabled: boolean;
  displayFilters: IIssueDisplayFilterOptions;
  handleDisplayFilterUpdate: (data: Partial<IIssueDisplayFilterOptions>) => void;
};

const COLUMN_LABELS: Partial<Record<keyof IIssueDisplayProperties, string>> = {
  state: "状态",
  priority: "优先级",
  assignee: "负责人",
  labels: "标签",
  start_date: "开始日期",
  due_date: "截止日期",
  estimate: "估算",
  created_on: "创建时间",
  updated_on: "更新时间",
  link: "链接",
  attachment_count: "附件",
  sub_issue_count: "子 Issue",
  modules: "模块",
  cycle: "周期",
};

export const SpreadsheetHeaderColumn = observer(function SpreadsheetHeaderColumn(props: TProps) {
  const { property } = props;
  const label = COLUMN_LABELS[property] ?? property;

  return (
    <th className="h-11 min-w-36 items-center border border-t-0 border-b-0 border-custom-border-200 bg-custom-background-90 py-1 text-left text-13 font-medium">
      <div className="flex items-center justify-between px-3">
        <span className="text-xs text-custom-text-300">{label}</span>
      </div>
    </th>
  );
});
