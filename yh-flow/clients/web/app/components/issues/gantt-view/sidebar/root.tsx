// FLOW: Forked from Plane gantt-chart/sidebar/root.tsx
// FLOW: Removed mobx-react observer wrapper
// FLOW: Removed @plane/i18n useTranslation, MultipleSelectGroupAction
// FLOW: Removed Row ERowVariant from @plane/ui
/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

import type { RefObject } from "react";
// components
import type { IBlockUpdateData } from "@plane/types";
import { cn } from "@plane/utils";
import type { TSelectionHelper } from "@/hooks/use-multiple-select";
// constants
import { HEADER_HEIGHT, SIDEBAR_WIDTH } from "../constants";

type Props = {
  blockIds: string[];
  blockUpdateHandler: (block: any, payload: IBlockUpdateData) => void;
  canLoadMoreBlocks?: boolean;
  loadMoreBlocks?: () => void;
  ganttContainerRef: RefObject<HTMLDivElement>;
  enableReorder: boolean | ((blockId: string) => boolean);
  enableSelection: boolean | ((blockId: string) => boolean);
  sidebarToRender: (props: any) => React.ReactNode;
  title: string;
  selectionHelpers: TSelectionHelper;
  showAllBlocks?: boolean;
  isEpic?: boolean;
};

export const GanttChartSidebar = function GanttChartSidebar(props: Props) {
  const {
    blockIds,
    blockUpdateHandler,
    enableReorder,
    enableSelection,
    sidebarToRender,
    loadMoreBlocks,
    canLoadMoreBlocks,
    ganttContainerRef,
    title,
    selectionHelpers,
    showAllBlocks = false,
    isEpic = false,
  } = props;

  return (
    <div
      // DO NOT REMOVE THE ID
      id="gantt-sidebar"
      className="sticky left-0 z-10 h-max min-h-full flex-shrink-0 border-r-[0.5px] border-subtle-1 bg-surface-1"
      style={{ width: `${SIDEBAR_WIDTH}px` }}
    >
      <div
        className="group/list-header sticky top-0 z-10 box-border flex flex-shrink-0 items-end justify-between gap-2 border-b-[0.5px] border-subtle-1 bg-surface-1 pr-4 pb-2 text-13 font-medium text-tertiary"
        style={{ height: `${HEADER_HEIGHT}px` }}
      >
        <div className={cn("flex items-center gap-2")}>
          <h6>{title}</h6>
        </div>
        <h6>Duration</h6>
      </div>

      <div className="h-max min-h-full bg-surface-1">
        {sidebarToRender &&
          sidebarToRender({
            title,
            blockUpdateHandler,
            blockIds,
            enableReorder,
            enableSelection,
            canLoadMoreBlocks,
            ganttContainerRef,
            loadMoreBlocks,
            selectionHelpers,
            showAllBlocks,
            isEpic,
          })}
      </div>
    </div>
  );
};
