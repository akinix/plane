// FLOW: Forked from Plane modules/dropdowns/order-by.tsx
// FLOW: ModuleOrderByDropdown — Module 列表排序切换
import React from "react";
import { ArrowUpDown } from "lucide-react";
import { MODULE_ORDER_BY_OPTIONS } from "@plane/constants";
import type { TModuleOrderByOptions } from "@plane/types";
import { CustomMenu } from "@plane/ui";

const ORDER_LABELS: Record<string, string> = {
  sort_order: "手动排序",
  name: "名称",
  created_at: "创建时间",
  updated_at: "更新时间",
};

type Props = {
  onChange: (value: TModuleOrderByOptions) => void;
  value: TModuleOrderByOptions | undefined;
};

export const ModuleOrderByDropdown = React.memo(function ModuleOrderByDropdown(props: Props) {
  const { onChange, value } = props;

  const orderByDetails = MODULE_ORDER_BY_OPTIONS.find((option) => value?.includes(option.key));
  const isDescending = value?.[0] === "-";
  const isManual = value?.includes("sort_order");

  const currentLabel = orderByDetails
    ? ORDER_LABELS[orderByDetails.key] ?? orderByDetails.key
    : "排序";

  return (
    <CustomMenu
      customButton={
        <div className="flex items-center gap-1 rounded-md border border-subtle px-2 py-1 text-12 text-tertiary hover:text-primary cursor-pointer">
          <ArrowUpDown className="size-3" />
          {currentLabel}
        </div>
      }
      placement="bottom-end"
      maxHeight="lg"
      closeOnSelect
    >
      {MODULE_ORDER_BY_OPTIONS.map((option) => (
        <CustomMenu.MenuItem
          key={option.key}
          className="flex items-center justify-between gap-2"
          onClick={() => {
            if (isDescending && !isManual) onChange(`-${option.key}` as TModuleOrderByOptions);
            else onChange(option.key as TModuleOrderByOptions);
          }}
        >
          <span>{ORDER_LABELS[option.key] ?? option.key}</span>
          {value?.includes(option.key) && (
            <span className="h-3 w-3 rounded-full bg-accent-primary" />
          )}
        </CustomMenu.MenuItem>
      ))}
      {!isManual && (
        <>
          <hr className="my-2 border-subtle" />
          <CustomMenu.MenuItem
            className="flex items-center justify-between gap-2"
            onClick={() => {
              if (isDescending) onChange(value.slice(1) as TModuleOrderByOptions);
            }}
          >
            升序
            {!isDescending && <span className="h-3 w-3 rounded-full bg-accent-primary" />}
          </CustomMenu.MenuItem>
          <CustomMenu.MenuItem
            className="flex items-center justify-between gap-2"
            onClick={() => {
              if (!isDescending) onChange(`-${value}` as TModuleOrderByOptions);
            }}
          >
            降序
            {isDescending && <span className="h-3 w-3 rounded-full bg-accent-primary" />}
          </CustomMenu.MenuItem>
        </>
      )}
    </CustomMenu>
  );
});
