// FLOW: Created for yh-flow — day-level gantt chart view
// FLOW: Uses same IWeekBlock structure as week view, with day-level column rendering
/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 *
 * Day chart view — similar to WeekChartView but with day-level time axis.
 * Each day block is rendered as an individual column at dayWidth scale.
 */

import { cn } from "@plane/utils";
// FLOW: use local useTimeLineChartStore hook
import { useTimeLineChartStore } from "../../hooks/use-timeline-chart";
//
import { HEADER_HEIGHT, SIDEBAR_WIDTH } from "../../constants";
import type { IWeekBlock } from "../../views";

export const DayChartView = function DayChartView() {
  const { currentViewData, renderView } = useTimeLineChartStore();
  const dayBlocks: IWeekBlock[] = renderView;

  if (!currentViewData) return null;

  return (
    <div className="absolute top-0 left-0 flex h-max min-h-full w-max">
      {dayBlocks?.map((block, rootIndex) => (
        <div
          key={`day-${block?.startDate.toString()}-${block?.endDate.toString()}`}
          className="relative flex flex-col outline-[0.25px] outline-subtle-1"
        >
          {/** 日期头 */}
          <div
            className="sticky top-0 z-10 flex-shrink-0 border-b-[0.5px] border-subtle-1 bg-surface-1"
            style={{ height: `${HEADER_HEIGHT}px` }}
          >
            <div className="flex h-full w-full items-center justify-center text-11 font-medium text-tertiary">
              <div className="flex flex-col items-center gap-0.5">
                <span className="text-11 uppercase">
                  {block.weekData?.shortTitle}
                </span>
                <span className="text-xs font-semibold text-primary">
                  {block.startDate.getDate()}
                </span>
              </div>
            </div>
          </div>
          {/** 行 */}
          <div className="relative h-full w-full" style={{ minWidth: `${currentViewData?.data?.dayWidth}px` }}>
            {block.children?.map((day) => (
              <div
                key={`day-col-${day.date.toString()}`}
                className={cn("h-full", {
                  "bg-accent-primary/5": day.today,
                })}
                style={{ width: `${currentViewData?.data?.dayWidth}px` }}
              />
            ))}
          </div>
        </div>
      ))}
    </div>
  );
};
