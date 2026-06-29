// FLOW: Forked from Plane gantt-chart/chart/root.tsx
// FLOW: Removed mobx-react observer, createPortal, useUserProfile, useTimeLineChartStore → local hook
// FLOW: Removed @plane/i18n, use hardcoded Chinese text
// FLOW: Simplified lazy-load on scroll (remove left/right infinite scroll)
/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

import { useEffect, useState } from "react";
// plane imports
import type { ChartDataType, IBlockUpdateData, IBlockUpdateDependencyData, TGanttViews } from "@plane/types";
import { cn } from "@plane/utils";
import { GanttChartHeader, GanttChartMainContent } from "./";
// FLOW: use local useTimeLineChartStore hook
import { useTimeLineChartStore } from "../hooks/use-timeline-chart";
// helpers
import { SIDEBAR_WIDTH } from "../constants";
import { currentViewDataWithView } from "../data";
import type { IMonthBlock, IMonthView, IWeekBlock } from "../views";
import { getNumberOfDaysBetweenTwoDates, monthView, quarterView, weekView } from "../views";

type ChartViewRootProps = {
  border: boolean;
  title: string;
  loaderTitle: string;
  blockIds: string[];
  blockUpdateHandler: (block: any, payload: IBlockUpdateData) => void;
  blockToRender: (data: any) => React.ReactNode;
  sidebarToRender: (props: any) => React.ReactNode;
  enableBlockLeftResize: boolean | ((blockId: string) => boolean);
  enableBlockRightResize: boolean | ((blockId: string) => boolean);
  enableBlockMove: boolean | ((blockId: string) => boolean);
  enableReorder: boolean | ((blockId: string) => boolean);
  enableAddBlock: boolean | ((blockId: string) => boolean);
  enableSelection: boolean | ((blockId: string) => boolean);
  enableDependency: boolean | ((blockId: string) => boolean);
  bottomSpacing: boolean;
  showAllBlocks: boolean;
  loadMoreBlocks?: () => void;
  updateBlockDates?: (updates: IBlockUpdateDependencyData[]) => Promise<void>;
  canLoadMoreBlocks?: boolean;
  quickAdd?: React.ReactNode | undefined;
  showToday: boolean;
  isEpic?: boolean;
};

const timelineViewHelpers = {
  day: weekView,
  week: weekView,
  month: monthView,
  quarter: quarterView,
};

export const ChartViewRoot = function ChartViewRoot(props: ChartViewRootProps) {
  const {
    border,
    title,
    blockIds,
    loadMoreBlocks,
    loaderTitle,
    blockUpdateHandler,
    sidebarToRender,
    blockToRender,
    canLoadMoreBlocks,
    enableBlockLeftResize,
    enableBlockRightResize,
    enableBlockMove,
    enableReorder,
    enableAddBlock,
    enableSelection,
    enableDependency,
    bottomSpacing,
    showAllBlocks,
    quickAdd,
    showToday,
    updateBlockDates,
    isEpic = false,
  } = props;
  // states
  const [itemsContainerWidth, setItemsContainerWidth] = useState(0);
  const [fullScreenMode, setFullScreenMode] = useState(false);
  // FLOW: use local useTimeLineChartStore
  const {
    currentView,
    currentViewData,
    renderView,
    updateCurrentView,
    updateCurrentViewData,
    updateRenderView,
    updateAllBlocksOnChartChangeWhileDragging,
  } = useTimeLineChartStore();

  const updateCurrentViewRenderPayload = (side: null | "left" | "right", view: TGanttViews, targetDate?: Date) => {
    const selectedCurrentView: TGanttViews = view;
    const selectedCurrentViewData: ChartDataType | undefined =
      selectedCurrentView && selectedCurrentView === currentViewData?.key
        ? currentViewData
        : currentViewDataWithView(view);

    if (selectedCurrentViewData === undefined) return;

    const currentViewHelpers = timelineViewHelpers[selectedCurrentView];
    const currentRender = currentViewHelpers.generateChart(selectedCurrentViewData, side, targetDate);
    const mergeRenderPayloads = currentViewHelpers.mergeRenderPayloads as (
      a: IWeekBlock[] | IMonthView | IMonthBlock[],
      b: IWeekBlock[] | IMonthView | IMonthBlock[]
    ) => IWeekBlock[] | IMonthView | IMonthBlock[];

    // updating the prevData, currentData and nextData
    if (currentRender.payload) {
      updateCurrentViewData(currentRender.state);

      if (side === "left") {
        updateCurrentView(selectedCurrentView);
        updateRenderView(mergeRenderPayloads(currentRender.payload, renderView));
        updateItemsContainerWidth(currentRender.scrollWidth);
        updateCurrentLeftScrollPosition(currentRender.scrollWidth);
        updateAllBlocksOnChartChangeWhileDragging(currentRender.scrollWidth);
        setItemsContainerWidth((prev) => prev + currentRender.scrollWidth);
      } else if (side === "right") {
        updateCurrentView(view);
        updateRenderView(mergeRenderPayloads(renderView, currentRender.payload));
        setItemsContainerWidth((prev) => prev + currentRender.scrollWidth);
      } else {
        updateCurrentView(view);
        updateRenderView(currentRender.payload);
        setItemsContainerWidth(currentRender.scrollWidth);
      }
    }

    return currentRender.state;
  };

  const handleToday = () => updateCurrentViewRenderPayload(null, currentView);

  // FLOW: simplified initial render - just call handleToday on mount
  useEffect(() => {
    handleToday();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const updateItemsContainerWidth = (width: number) => {
    const scrollContainer = document.querySelector("#gantt-container") as HTMLDivElement;
    if (!scrollContainer) return;
    setItemsContainerWidth(width + scrollContainer?.scrollLeft);
  };

  const updateCurrentLeftScrollPosition = (width: number) => {
    const scrollContainer = document.querySelector("#gantt-container") as HTMLDivElement;
    if (!scrollContainer) return;
    scrollContainer.scrollLeft = width + scrollContainer?.scrollLeft;
  };

  const content = (
    <div
      className={cn("shadow relative flex h-full flex-col rounded-xs bg-surface-1 select-none", {
        "inset-0 z-[25] bg-surface-1": fullScreenMode,
        "border-[0.5px] border-subtle": border,
      })}
    >
      <GanttChartHeader
        blockIds={blockIds}
        fullScreenMode={fullScreenMode}
        toggleFullScreenMode={() => setFullScreenMode((prevData) => !prevData)}
        handleChartView={(key) => updateCurrentViewRenderPayload(null, key)}
        handleToday={handleToday}
        loaderTitle={loaderTitle}
        showToday={showToday}
      />
      <GanttChartMainContent
        blockIds={blockIds}
        loadMoreBlocks={loadMoreBlocks}
        canLoadMoreBlocks={canLoadMoreBlocks}
        blockToRender={blockToRender}
        blockUpdateHandler={blockUpdateHandler}
        bottomSpacing={bottomSpacing}
        enableBlockLeftResize={enableBlockLeftResize}
        enableBlockMove={enableBlockMove}
        enableBlockRightResize={enableBlockRightResize}
        enableReorder={enableReorder}
        enableSelection={enableSelection}
        enableAddBlock={enableAddBlock}
        enableDependency={enableDependency}
        itemsContainerWidth={itemsContainerWidth}
        showAllBlocks={showAllBlocks}
        sidebarToRender={sidebarToRender}
        title={title}
        updateCurrentViewRenderPayload={updateCurrentViewRenderPayload}
        quickAdd={quickAdd}
        updateBlockDates={updateBlockDates}
        isEpic={isEpic}
      />
    </div>
  );

  return fullScreenMode ? content : content;
};
