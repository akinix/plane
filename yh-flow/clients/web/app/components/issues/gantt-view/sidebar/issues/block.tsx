// FLOW: Forked from Plane gantt-chart/sidebar/issues/block.tsx
// FLOW: Removed mobx-react observer wrapper
// FLOW: Removed useIssueDetail, MultipleSelectEntityAction, Row from @plane/ui
// FLOW: Removed IssueGanttSidebarBlock import, render inline title
/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// plane imports
import type { IGanttBlock } from "@plane/types";
import { cn } from "@plane/utils";
// hooks
import type { TSelectionHelper } from "@/hooks/use-multiple-select";
// local imports
import { useTimeLineChartStore } from "../../hooks/use-timeline-chart";
import { BLOCK_HEIGHT } from "../../constants";

type Props = {
  block: IGanttBlock;
  enableSelection: boolean;
  isDragging: boolean;
  selectionHelpers?: TSelectionHelper;
  isEpic?: boolean;
};

export const IssuesSidebarBlock = function IssuesSidebarBlock(props: Props) {
  const { block, enableSelection, isDragging, selectionHelpers, isEpic = false } = props;
  // store hooks
  const { updateActiveBlockId, isBlockActive, getNumberOfDaysFromPosition } = useTimeLineChartStore();

  const isBlockComplete = !!block?.start_date && !!block?.target_date;
  const duration = isBlockComplete ? getNumberOfDaysFromPosition(block?.position?.width) : undefined;

  if (!block?.data) return null;

  const isBlockHoveredOn = isBlockActive(block.id);

  return (
    <div
      className={cn("group/list-block", {
        "rounded-sm bg-layer-1": isDragging,
      })}
      onMouseEnter={() => updateActiveBlockId(block.id)}
      onMouseLeave={() => updateActiveBlockId(null)}
    >
      <div
        className={cn(
          "group flex w-full items-center gap-2 bg-layer-transparent pr-4 hover:bg-layer-transparent-hover",
          { "bg-layer-transparent-hover": isBlockHoveredOn }
        )}
        style={{ height: `${BLOCK_HEIGHT}px` }}
      >
        <div className="flex h-full flex-grow items-center justify-between gap-2 truncate">
          <div className="flex-grow truncate">
            <span className="text-13 text-secondary">
              {block.data.name || "Untitled"}
            </span>
          </div>
          {duration && (
            <div className="flex-shrink-0 text-13 text-secondary">
              <span>{duration} day{duration > 1 ? "s" : ""}</span>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
