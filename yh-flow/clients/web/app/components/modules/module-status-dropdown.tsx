// FLOW: Forked from Plane modules/module-status-dropdown.tsx
// FLOW: ModuleStatusDropdown — Module 状态下拉选择
import React from "react";
import { MODULE_STATUS } from "@plane/constants";
import type { IModule } from "@plane/types";
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
  onChange?: (status: string) => void;
  isDisabled?: boolean;
  moduleDetails?: IModule;
  handleModuleDetailsChange?: (payload: Partial<IModule>) => Promise<void>;
};

export const ModuleStatusDropdown = React.memo(function ModuleStatusDropdown(props: Props) {
  const { value, onChange, isDisabled = false, moduleDetails, handleModuleDetailsChange } = props;

  // Support both standalone (value/onChange) and store-based (moduleDetails/handleModuleDetailsChange) usage
  const currentStatus = moduleDetails?.status ?? value ?? "backlog";
  const moduleStatus = MODULE_STATUS.find((status) => status.value === currentStatus);

  if (!moduleStatus) return <></>;

  const handleChange = (val: string) => {
    if (onChange) {
      onChange(val as any);
    } else if (handleModuleDetailsChange) {
      handleModuleDetailsChange({ status: val as any });
    }
  };

  return (
    <CustomSelect
      customButton={
        <span
          className={`flex h-6 w-20 items-center justify-center rounded-sm text-center text-11 ${
            isDisabled ? "cursor-not-allowed" : "cursor-pointer"
          }`}
          style={{
            color: moduleStatus.color ?? "#a3a3a2",
            backgroundColor: moduleStatus.color ? `${moduleStatus.color}20` : "#a3a3a220",
          }}
        >
          {STATUS_LABELS[currentStatus] ?? "待开始"}
        </span>
      }
      value={moduleStatus?.value}
      onChange={handleChange}
      disabled={isDisabled}
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
