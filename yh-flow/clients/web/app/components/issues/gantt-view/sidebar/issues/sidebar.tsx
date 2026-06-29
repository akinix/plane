// FLOW: Forked from Plane gantt-chart/sidebar/issues/sidebar.tsx
// FLOW: Removed mobx-react observer wrapper
// FLOW: Removed RenderIfVisible HOC, useIntersectionObserver
// FLOW: Removed useIssuesStore dependency
// FLOW: Simplified sidebar rendering
/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

import type { RefObject } from "react";
// ui
import type { IBlockUpdateData } from "@plane/types";
import { Loader } from "@plane/ui";
// hooks
import type { TSelectionHelper } from "@/hooks/use-multiple-select";
// local imports
import { useTimeLineChartStore } from "../../hooks/use-timeline-chart";
import { IssuesSidebarBlock } from "./block";

type Props = {
  blockUpdateHandler: (block: any, payload: IBlockUpdateData) => void;
  canLoadMoreBlocks?: boolean;
  loadMoreBlocks?: () => void;
  ganttContainerRef: RefObject<HTMLDivElement>;
  blockIds: string[];
  enableReorder: boolean;
  enableSelection: boolean;
  showAllBlocks?: boolean;
  selectionHelpers?: TSelectionHelper;
  isEpic?: boolean;
};

export const IssueGanttSidebar = function IssueGanttSidebar(props: Props) {
  const {
    blockUpdateHandler,
    blockIds,
    enableReorder,
    enableSelection,
    loadMoreBlocks,
    canLoadMoreBlocks,
    ganttContainerRef,
    showAllBlocks = false,
    selectionHelpers,
    isEpic = false,
  } = props;

  const { getBlockById } = useTimeLineChartStore();

  const handleOnDrop = (
    _draggingBlockId: string | undefined,
    _droppedBlockId: string | undefined,
    _dropAtEndOfList: boolean
  ) => {
    // FLOW: simplified — reorder not implemented
  };

  return (
    <div>
      {blockIds ? (
        <>
          {blockIds.map((blockId, index) => {
            const block = getBlockById(blockId);
            const isBlockVisibleOnSidebar = block?.start_date && block?.target_date;

            if (!block || (!showAllBlocks && !isBlockVisibleOnSidebar)) return null;

            return (
              <IssuesSidebarBlock
                key={block.id}
                block={block}
                enableSelection={enableSelection}
                isDragging={false}
                selectionHelpers={selectionHelpers}
                isEpic={isEpic}
              />
            );
          })}
          {canLoadMoreBlocks && (
            <div className="p-2">
              <div className="flex h-10 w-full animate-pulse items-center justify-between gap-1.5 rounded-sm bg-layer-1 px-4 py-1.5 md:h-8 md:px-1" />
            </div>
          )}
        </>
      ) : (
        <Loader className="space-y-3 pr-2">
          <Loader.Item height="34px" />
          <Loader.Item height="34px" />
          <Loader.Item height="34px" />
          <Loader.Item height="34px" />
        </Loader>
      )}
    </div>
  );
};
