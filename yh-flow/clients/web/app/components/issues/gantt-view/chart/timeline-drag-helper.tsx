// FLOW: Forked from Plane gantt-chart/chart/timeline-drag-helper.tsx
// FLOW: Removed useAutoScroller dependency (simplified)
/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

import type { RefObject } from "react";
// FLOW: use local useTimeLineChartStore hook
import { useTimeLineChartStore } from "../hooks/use-timeline-chart";

type Props = {
  ganttContainerRef: RefObject<HTMLDivElement>;
};

export const TimelineDragHelper = function TimelineDragHelper(props: Props) {
  const { ganttContainerRef } = props;
  const { isDragging } = useTimeLineChartStore();

  // FLOW: Auto-scroll on drag is simplified. Use auto scroller from atlaskit if needed.
  // useAutoScroller(ganttContainerRef, isDragging, SIDEBAR_WIDTH, HEADER_HEIGHT);
  return <></>;
};
