// FLOW: Forked from Plane modules/select/status.tsx
// FLOW: ModuleStatusSelect — Module 状态选择器（用于 Issue 属性面板）
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
  isOpen?: boolean;
  onOpen?: () => void;
};

export const ModuleStatusSelect = React.memo(function ModuleStatusSelect(props: Props) {
  const { value, onChange } = props;

  const selectedValue = MODULE_STATUS.find((s) => s.value === value);

  return (
    <CustomSelect
      value={value}
      label={
        <div className="flex items-center justify-center gap-2 py-0.5 text-11">
          {value ? (
            <span
              className="h-2 w-2 rounded-full"
              style={{ backgroundColor: selectedValue?.color ?? "#a3a3a2" }}
            />
          ) : (
            <span className="text-secondary">状态</span>
          )}
          {selectedValue ? STATUS_LABELS[value ?? ""] ?? value : "状态"}
        </div>
      }
      onChange={(val: string) => onChange(val)}
      noChevron
    >
      {MODULE_STATUS.map((status) => (
        <CustomSelect.Option key={status.value} value={status.value}>
          <div className="flex items-center gap-2">
            <span
              className="h-2 w-2 rounded-full"
              style={{ backgroundColor: status.color ?? "#a3a3a2" }}
            />
            {STATUS_LABELS[status.value] ?? status.value}
          </div>
        </CustomSelect.Option>
      ))}
    </CustomSelect>
  );
});
