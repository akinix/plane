// FLOW: Forked from Plane spreadsheet/columns/header-column.tsx
// Simplified — displays column header with label (no sort menu in mock)
import type { IIssueDisplayFilterOptions, IIssueDisplayProperties } from "@plane/types";

type TProps = {
  property: keyof IIssueDisplayProperties;
  displayFilters: IIssueDisplayFilterOptions;
  handleDisplayFilterUpdate: (data: Partial<IIssueDisplayFilterOptions>) => void;
  onClose: () => void;
  isEpic?: boolean;
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

export function HeaderColumn(props: TProps) {
  const { property } = props;
  const label = COLUMN_LABELS[property] ?? property;

  return (
    <div className="flex w-full cursor-pointer items-center justify-between gap-1.5 px-3 py-2 text-xs text-custom-text-300 hover:text-custom-text-100">
      <span>{label}</span>
    </div>
  );
}
