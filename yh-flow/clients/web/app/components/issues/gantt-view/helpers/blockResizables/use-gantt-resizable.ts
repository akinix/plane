// FLOW: Forked from Plane gantt-chart/helpers/blockResizables/use-gantt-resizable.ts
// FLOW: Removed @plane/propel/toast dependency
// FLOW: Simplified to remove strict ref guard (for now — will add in future for threat T-17-GANT-02)
/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

import { useRef, useState } from "react";
// Plane
import type { IBlockUpdateDependencyData, IGanttBlock } from "@plane/types";
// FLOW: use local hook
import { useTimeLineChartStore } from "../../hooks/use-timeline-chart";
import { SIDEBAR_WIDTH } from "../../constants";

export const useGanttResizable = (
  block: IGanttBlock,
  resizableRef: React.RefObject<HTMLDivElement>,
  ganttContainerRef: React.RefObject<HTMLDivElement>,
  updateBlockDates?: (updates: IBlockUpdateDependencyData[]) => Promise<void>
) => {
  const initialPositionRef = useRef<{ marginLeft: number; width: number; offsetX: number }>({
    marginLeft: 0,
    width: 0,
    offsetX: 0,
  });
  const ganttContainerDimensions = useRef<DOMRect | undefined>();
  const currMouseEvent = useRef<MouseEvent | undefined>();
  const { currentViewData, updateBlockPosition, setIsDragging, getUpdatedPositionAfterDrag } = useTimeLineChartStore();
  const [isMoving, setIsMoving] = useState<"left" | "right" | "move" | undefined>();

  // FLOW: StrictMode ref guard omitted per T-17-GANT-02 (added in future iteration)
  // handle block resize from the left end
  const handleBlockDrag = (
    e: React.MouseEvent<HTMLDivElement, MouseEvent>,
    dragDirection: "left" | "right" | "move"
  ) => {
    const ganttContainerElement = ganttContainerRef.current;
    if (!currentViewData || !resizableRef.current || !block.position || !ganttContainerElement) return;

    if (e.button !== 0) return;

    const resizableDiv = resizableRef.current;
    ganttContainerDimensions.current = ganttContainerElement.getBoundingClientRect();

    const dayWidth = currentViewData.data.dayWidth;
    const mouseX = e.clientX - ganttContainerDimensions.current.left - SIDEBAR_WIDTH + ganttContainerElement.scrollLeft;

    initialPositionRef.current = {
      width: block.position.width ?? 0,
      marginLeft: block.position.marginLeft ?? 0,
      offsetX: mouseX - block.position.marginLeft,
    };

    const handleOnScroll = () => {
      if (currMouseEvent.current) handleMouseMove(currMouseEvent.current);
    };

    const handleMouseMove = (e: MouseEvent) => {
      currMouseEvent.current = e;
      setIsMoving(dragDirection);
      setIsDragging(true);
      if (!ganttContainerDimensions.current) return;

      const { left: containerLeft } = ganttContainerDimensions.current;
      const mouseX = e.clientX - containerLeft - SIDEBAR_WIDTH + ganttContainerElement.scrollLeft;

      let width = initialPositionRef.current.width;
      let marginLeft = initialPositionRef.current.marginLeft;

      if (dragDirection === "left") {
        marginLeft = Math.round(mouseX / dayWidth) * dayWidth;
        const prevMarginLeft = parseFloat(resizableDiv.style.marginLeft.slice(0, -2));
        const prevWidth = parseFloat(resizableDiv.style.width.slice(0, -2));
        const marginDelta = prevMarginLeft - marginLeft;
        width = block.target_date ? prevWidth + marginDelta : 60; // DEFAULT_BLOCK_WIDTH
      } else if (dragDirection === "right") {
        width = Math.round(mouseX / dayWidth) * dayWidth - marginLeft;
        if (!block.start_date) {
          const marginRight = Math.round(mouseX / dayWidth) * dayWidth;
          marginLeft = marginRight - 60;
          width = 60;
        }
      } else if (dragDirection === "move") {
        marginLeft = Math.round((mouseX - initialPositionRef.current.offsetX) / dayWidth) * dayWidth;
      }

      if (width < dayWidth) return;

      resizableDiv.style.width = `${width}px`;
      resizableDiv.style.marginLeft = `${marginLeft}px`;

      const deltaLeft = Math.round((marginLeft - (block.position?.marginLeft ?? 0)) / dayWidth) * dayWidth;
      const deltaWidth = Math.round((width - (block.position?.width ?? 0)) / dayWidth) * dayWidth;

      if (deltaWidth || deltaLeft) updateBlockPosition(block.id, deltaLeft, deltaWidth);
    };

    const handleMouseUp = () => {
      setIsMoving(undefined);
      document.removeEventListener("mousemove", handleMouseMove);
      ganttContainerElement.removeEventListener("scroll", handleOnScroll);
      document.removeEventListener("mouseup", handleMouseUp);

      try {
        const blockUpdates = getUpdatedPositionAfterDrag(block.id, false);
        if (updateBlockDates) updateBlockDates(blockUpdates);
      } catch {
        // FLOW: simplified error handling
        console.error("Error updating block dates");
      }
      setIsDragging(false);
    };

    document.addEventListener("mousemove", handleMouseMove);
    ganttContainerElement.addEventListener("scroll", handleOnScroll);
    document.addEventListener("mouseup", handleMouseUp);
  };

  return { isMoving, handleBlockDrag };
};
