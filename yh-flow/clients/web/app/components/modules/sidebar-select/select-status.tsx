// FLOW: Forked from Plane modules/sidebar-select/select-status.tsx
// FLOW: SidebarStatusSelect — Module 侧栏状态选择器（用于 Module 详情页侧栏）
import React from "react";
import { MODULE_STATUS } from "@plane/constants";
import { CustomSelect } from "@plane/ui";

const STATUS_LABELS: Record<string, string> = {
  backlog: "待开始",
  planned: "待开始",
  in_progress: "进行中",
  completed: "已完成",
  cancelled: "已取消",
};

type Props = {
  value?: string;
  onChange: (status: string) => void;
};

export const SidebarStatusSelect = React.memo(function SidebarStatusSelect(props: Props) {
  const { value, onChange } = props;

  return (
    <div className="flex flex-wrap items-center py-2">
      <div className="flex items-center gap-x-2 text-13 sm:basis-1/2">
        <span className="h-4 w-4 flex-shrink-0 rounded-full border border-subtle" />
        <span>状态</span>
      </div>
      <div className="sm:basis-1/2">
        <CustomSelect
          label={
            <span className={`flex items-center gap-2 text-left capitalize ${value ? "" : "text-primary"}`}>
              <span
                className="h-2 w-2 flex-shrink-0 rounded-full"
                style={{
                  backgroundColor: MODULE_STATUS?.find((option) => option.value === value)?.color ?? "#a3a3a2",
                }}
              />
              {STATUS_LABELS[value ?? ""] ?? value ?? "待开始"}
            </span>
          }
          value={value}
          onChange={(val: string) => onChange(val)}
        >
          {MODULE_STATUS.map((option) => (
            <CustomSelect.Option key={option.value} value={option.value}>
              <div className="flex items-center gap-2">
                <span className="h-2 w-2 flex-shrink-0 rounded-full" style={{ backgroundColor: option.color }} />
                {STATUS_LABELS[option.value] ?? option.value}
              </div>
            </CustomSelect.Option>
          ))}
        </CustomSelect>
      </div>
    </div>
  );
});
