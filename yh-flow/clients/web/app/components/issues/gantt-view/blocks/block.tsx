// FLOW: Forked from Plane gantt-chart/blocks/block.tsx
// FLOW: Removed mobx-react observer wrapper
// FLOW: Removed RenderIfVisible HOC
// FLOW: Removed plane-web ChartDraggable dependency handle
/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

import type { RefObject } from "react";
import { useRef } from "react";
// FLOW: use local hook
import { useTimeLineChartStore } from "../hooks/use-timeline-chart";
// constants
import { BLOCK_HEIGHT } from "../constants";
// components
import { ChartDraggable } from "../helpers";

type Props = {
  blockId: string;
  showAllBlocks: boolean;
  blockToRender: (data: any) => React.ReactNode;
  enableBlockLeftResize: boolean;
  enableBlockRightResize: boolean;
  enableBlockMove: boolean;
  enableDependency: boolean;
  ganttContainerRef: RefObject<HTMLDivElement>;
  updateBlockDates?: (updates: any) => Promise<void>;
};

export const GanttChartBlock = function GanttChartBlock(props: Props) {
  const {
    blockId,
    showAllBlocks,
    blockToRender,
    enableBlockLeftResize,
    enableBlockRightResize,
    enableBlockMove,
    ganttContainerRef,
    enableDependency,
    updateBlockDates,
  } = props;
  // store hooks
  const { updateActiveBlockId, getBlockById, getIsCurrentDependencyDragging, currentView } = useTimeLineChartStore();
  // refs
  const resizableRef = useRef<HTMLDivElement>(null);

  const block = getBlockById(blockId);

  const isCurrentDependencyDragging = getIsCurrentDependencyDragging(blockId);

  // FLOW: useGanttResizable not used in simplified block — drag handled at view level
  const isMoving: undefined = undefined;

  const isBlockVisibleOnChart = block?.start_date || block?.target_date;

  // hide the block if it doesn't have start and target dates and showAllBlocks is false
  if (!block || (!showAllBlocks && !isBlockVisibleOnChart)) return null;

  if (!block.data) return null;

  return (
    <div
      className="relative z-[5]"
      style={{
        height: `${BLOCK_HEIGHT}px`,
        marginLeft: `${block.position?.marginLeft}px`,
        width: `${block.position?.width}px`,
      }}
    >
      {isBlockVisibleOnChart && (
        <div className="flex h-full w-full items-center">
          <div
            className="relative h-full w-full"
            onMouseEnter={() => updateActiveBlockId(blockId)}
            onMouseLeave={() => updateActiveBlockId(null)}
          >
            <ChartDraggable
              block={block}
              blockToRender={blockToRender}
              handleBlockDrag={() => {}}
              enableBlockLeftResize={enableBlockLeftResize}
              enableBlockRightResize={enableBlockRightResize}
              enableBlockMove={enableBlockMove}
              enableDependency={enableDependency}
              isMoving={isMoving}
              ganttContainerRef={ganttContainerRef}
            />
          </div>
        </div>
      )}
    </div>
  );
};
