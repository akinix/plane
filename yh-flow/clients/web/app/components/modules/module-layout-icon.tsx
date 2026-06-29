// FLOW: Forked from Plane modules/module-layout-icon.tsx
// FLOW: ModuleLayoutIcon — Module 视图布局图标切换
import * as React from "react";
import { LayoutGrid, List, GanttChartSquare } from "lucide-react";
import type { TModuleLayoutOptions } from "@plane/types";
import { cn } from "@plane/utils";

interface ILayoutIcon {
  className?: string;
  containerClassName?: string;
  layoutType: TModuleLayoutOptions;
  size?: number;
  withContainer?: boolean;
}

export function ModuleLayoutIcon(props: ILayoutIcon) {
  const { layoutType, className = "", containerClassName = "", size = 14, withContainer = false } = props;

  const icons: Record<string, React.ComponentType<{ width?: number; height?: number; className?: string }>> = {
    list: List as any,
    board: LayoutGrid as any,
    gantt: GanttChartSquare as any,
  };
  const Icon = icons[layoutType ?? "list"];

  if (!Icon) return null;

  return (
    <>
      {withContainer ? (
        <div
          className={cn("flex flex-shrink-0 items-center justify-center rounded-sm border p-0.5", containerClassName)}
        >
          <Icon width={size} height={size} className={cn(className)} />
        </div>
      ) : (
        <Icon width={size} height={size} className={cn("flex-shrink-0", className)} />
      )}
    </>
  );
}
