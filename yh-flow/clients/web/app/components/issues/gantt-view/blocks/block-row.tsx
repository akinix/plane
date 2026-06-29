// FLOW: Forked from Plane gantt-chart/blocks/block-row.tsx
// FLOW: Removed mobx-react observer wrapper
// FLOW: Removed useIssueDetail hook dependency
// FLOW: Simplified block row rendering
/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

import { ArrowRight } from "lucide-react";
// helpers
import { cn } from "@plane/utils";
// hooks
// FLOW: use local hook
import { useTimeLineChartStore } from "../hooks/use-timeline-chart";
//
import { BLOCK_HEIGHT, SIDEBAR_WIDTH } from "../constants";

type Props = {
  blockId: string;
  showAllBlocks: boolean;
  handleScrollToBlock: (block: any) => void;
  ganttContainerRef: React.RefObject<HTMLDivElement>;
};

export const BlockRow = function BlockRow(props: Props) {
  const { blockId, showAllBlocks, handleScrollToBlock } = props;
  // store hooks
  const { getBlockById, updateActiveBlockId, isBlockActive } = useTimeLineChartStore();

  const block = getBlockById(blockId);

  // hide the block if it doesn't have start and target dates and showAllBlocks is false
  if (!block || !block.data || (!showAllBlocks && !(block.start_date && block.target_date))) return null;

  const isBlockVisibleOnChart = block.start_date || block.target_date;
  const isBlockHoveredOn = isBlockActive(block.id);

  return (
    <div
      className="relative w-max min-w-full"
      onMouseEnter={() => updateActiveBlockId(blockId)}
      onMouseLeave={() => updateActiveBlockId(null)}
      style={{ height: `${BLOCK_HEIGHT}px` }}
    >
      <div
        className={cn("relative h-full bg-layer-transparent hover:bg-layer-transparent-hover", {
          "bg-layer-transparent-hover": isBlockHoveredOn,
        })}
      >
        {isBlockVisibleOnChart && (
          <button
            type="button"
            className="sticky z-[5] grid h-8 w-8 translate-y-1.5 cursor-pointer place-items-center rounded-sm border border-strong bg-layer-1 text-secondary hover:text-primary"
            style={{ left: `${SIDEBAR_WIDTH + 4}px` }}
            onClick={() => block && handleScrollToBlock(block)}
          >
            <ArrowRight className="h-3.5 w-3.5" />
          </button>
        )}
      </div>
    </div>
  );
};
