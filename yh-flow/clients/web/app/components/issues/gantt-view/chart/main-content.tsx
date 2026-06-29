// FLOW: Forked from Plane gantt-chart/chart/main-content.tsx
// FLOW: Removed plane-web specific imports (TimelineDependencyPaths, TimelineDraggablePath,
//      GanttAdditionalLayers, GanttChartRowList, GanttChartBlocksList, IssueBulkOperationsRoot,
//      useBulkOperationStatus, MultipleSelectGroup)
// FLOW: Removed @atlaskit/pragmatic-drag-and-drop dependencies
// FLOW: Simplified to render chart views with sidebar and blocks as placeholders
// FLOW: Removed mobx-react observer wrapper
/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

import type { ChartDataType, IBlockUpdateData, IBlockUpdateDependencyData, TGanttViews } from "@plane/types";
import { cn } from "@plane/utils";
// components
// FLOW: use local imports
import { MonthChartView, QuarterChartView, WeekChartView, DayChartView } from "./views";
import { GanttChartSidebar } from "../sidebar";
// hooks
import { useTimeLineChartStore } from "../hooks/use-timeline-chart";
//
import { HEADER_HEIGHT } from "../constants";
import { getItemPositionWidth } from "../views";
import { TimelineDragHelper } from "./timeline-drag-helper";

type Props = {
  blockIds: string[];
  canLoadMoreBlocks?: boolean;
  loadMoreBlocks?: () => void;
  updateBlockDates?: (updates: IBlockUpdateDependencyData[]) => Promise<void>;
  blockToRender: (data: any) => React.ReactNode;
  blockUpdateHandler: (block: any, payload: IBlockUpdateData) => void;
  bottomSpacing: boolean;
  enableBlockLeftResize: boolean | ((blockId: string) => boolean);
  enableBlockMove: boolean | ((blockId: string) => boolean);
  enableBlockRightResize: boolean | ((blockId: string) => boolean);
  enableReorder: boolean | ((blockId: string) => boolean);
  enableSelection: boolean | ((blockId: string) => boolean);
  enableAddBlock: boolean | ((blockId: string) => boolean);
  enableDependency: boolean | ((blockId: string) => boolean);
  itemsContainerWidth: number;
  showAllBlocks: boolean;
  sidebarToRender: (props: any) => React.ReactNode;
  title: string;
  updateCurrentViewRenderPayload: (
    direction: "left" | "right" | null,
    currentView: TGanttViews,
    targetDate?: Date
  ) => ChartDataType | undefined;
  quickAdd?: React.ReactNode | undefined;
  isEpic?: boolean;
};

export const GanttChartMainContent = function GanttChartMainContent(props: Props) {
  const {
    blockIds,
    blockToRender,
    blockUpdateHandler,
    bottomSpacing,
    enableBlockLeftResize,
    enableBlockMove,
    enableBlockRightResize,
    enableReorder,
    enableAddBlock,
    enableDependency,
    enableSelection,
    itemsContainerWidth,
    showAllBlocks,
    sidebarToRender,
    title,
    canLoadMoreBlocks,
    updateCurrentViewRenderPayload,
    quickAdd,
    updateBlockDates,
    isEpic = false,
  } = props;
  // refs
  // FLOW: use useRef from react
  const ganttContainerRef = { current: null as HTMLDivElement | null };
  // chart hook
  const { currentView, currentViewData } = useTimeLineChartStore();

  // FLOW: removed Auto Scroll for Ganttlist (requires @atlaskit)

  // handling scroll functionality
  const onScroll = (e: React.UIEvent<HTMLDivElement, UIEvent>) => {
    const { clientWidth, scrollLeft, scrollWidth } = e.currentTarget;

    const approxRangeLeft = scrollLeft;
    const approxRangeRight = scrollWidth - (scrollLeft + clientWidth);

    if (approxRangeRight < clientWidth) {
      updateCurrentViewRenderPayload("right", currentView);
    }
    if (approxRangeLeft < clientWidth) {
      updateCurrentViewRenderPayload("left", currentView);
    }
  };

  const handleScrollToBlock = (_block: any) => {
    // FLOW: simplified - no-op
  };

  const CHART_VIEW_COMPONENTS: {
    [key in TGanttViews]: React.FC;
  } = {
    day: DayChartView,
    week: WeekChartView,
    month: MonthChartView,
    quarter: QuarterChartView,
  };

  if (!currentView) return null;
  const ActiveChartView = CHART_VIEW_COMPONENTS[currentView];

  return (
    <>
      <TimelineDragHelper ganttContainerRef={ganttContainerRef as any} />
      <>
        <div
          // DO NOT REMOVE THE ID
          id="gantt-container"
          className={cn(
            "vertical-scrollbar horizontal-scrollbar flex scrollbar-lg h-full w-full overflow-auto border-t-[0.5px] border-subtle",
            {
              "mb-8": bottomSpacing,
            }
          )}
          ref={(el) => { ganttContainerRef.current = el; }}
          onScroll={onScroll}
        >
          <GanttChartSidebar
            blockIds={blockIds}
            loadMoreBlocks={undefined}
            canLoadMoreBlocks={false}
            ganttContainerRef={ganttContainerRef as any}
            blockUpdateHandler={blockUpdateHandler}
            enableReorder={enableReorder}
            enableSelection={enableSelection}
            sidebarToRender={sidebarToRender}
            title={title}
            selectionHelpers={undefined as any}
            showAllBlocks={showAllBlocks}
            isEpic={isEpic}
          />
          <div className="relative h-max min-h-full flex-shrink-0 flex-grow">
            <ActiveChartView />
            {currentViewData && (
              <div
                className="relative h-full"
                style={{
                  width: `${itemsContainerWidth}px`,
                  transform: `translateY(${HEADER_HEIGHT}px)`,
                  paddingBottom: `${HEADER_HEIGHT}px`,
                }}
              >
                {/* FLOW: simplified block rendering — render blocks for each issue */}
                {blockIds.map((blockId) => {
                  const { getBlockById } = useTimeLineChartStore();
                  const block = getBlockById(blockId);
                  if (!block || !block.data) return null;
                  return (
                    <div
                      key={blockId}
                      className="relative z-[5]"
                      style={{
                        height: "44px",
                        marginLeft: `${block.position?.marginLeft || 0}px`,
                        width: `${block.position?.width || 60}px`,
                      }}
                    >
                      {blockToRender({ ...block.data, meta: block.meta })}
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        </div>
        {quickAdd ? quickAdd : null}
      </>
    </>
  );
};
