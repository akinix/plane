// FLOW: Forked from Plane gantt-chart/helpers/add-block.tsx
// FLOW: Removed @plane/propel icons/tooltip dependencies
/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

import { useEffect, useRef, useState } from "react";
import { addDays } from "date-fns";
// FLOW: use local useTimeLineChartStore hook
import { useTimeLineChartStore } from "../hooks/use-timeline-chart";

type Props = {
  block: any;
  blockUpdateHandler: (block: any, payload: any) => void;
};

export const ChartAddBlock = function ChartAddBlock(props: Props) {
  const { block, blockUpdateHandler } = props;
  // states
  const [isButtonVisible, setIsButtonVisible] = useState(false);
  const [buttonXPosition, setButtonXPosition] = useState(0);
  const [buttonStartDate, setButtonStartDate] = useState<Date | null>(null);
  // refs
  const containerRef = useRef<HTMLDivElement>(null);
  // chart hook
  const { currentViewData, currentView } = useTimeLineChartStore();

  const handleButtonClick = () => {
    if (!currentViewData) return;

    const { startDate: chartStartDate, dayWidth } = currentViewData.data;
    const columnNumber = buttonXPosition / dayWidth;

    let numberOfDays = 1;
    if (currentView === "quarter") numberOfDays = 7;

    const startDate = addDays(chartStartDate, columnNumber);
    const endDate = addDays(startDate, numberOfDays);

    blockUpdateHandler(block.data, {
      start_date: startDate.toISOString() ?? undefined,
      target_date: endDate.toISOString() ?? undefined,
      meta: block.meta,
    });
  };

  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    const handleMouseMove = (e: MouseEvent) => {
      if (!currentViewData) return;
      setButtonXPosition(e.offsetX);
    };

    container.addEventListener("mousemove", handleMouseMove);
    return () => {
      container?.removeEventListener("mousemove", handleMouseMove);
    };
  }, [buttonXPosition, currentViewData]);

  return (
    <div
      className="relative h-full w-full"
      onMouseEnter={() => setIsButtonVisible(true)}
      onMouseLeave={() => setIsButtonVisible(false)}
    >
      <div ref={containerRef} className="h-full w-full" />
      {isButtonVisible && (
        <button
          type="button"
          className="absolute top-1/2 grid h-8 w-8 -translate-x-1/2 -translate-y-1/2 place-items-center rounded-sm border border-strong bg-layer-1 p-1.5 text-secondary hover:text-primary"
          style={{
            marginLeft: `${buttonXPosition}px`,
          }}
          onClick={handleButtonClick}
        >
          <svg className="h-3.5 w-3.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M12 5v14M5 12h14" />
          </svg>
        </button>
      )}
    </div>
  );
};
