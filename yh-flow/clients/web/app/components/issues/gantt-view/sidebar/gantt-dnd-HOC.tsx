// FLOW: Forked from Plane gantt-chart/sidebar/gantt-dnd-HOC.tsx
// FLOW: Removed all @atlaskit/pragmatic-drag-and-drop dependencies
// FLOW: Simplified to a plain wrapper without drag/drop functionality
// FLOW: Removed @plane/hooks useOutsideClickDetector
// FLOW: Removed @plane/propel/toast
/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// FLOW: Simplified DnD HOC — no actual drag/drop, just renders children
type Props = {
  id: string;
  isLastChild: boolean;
  isDragEnabled: boolean;
  children: (isDragging: boolean) => React.ReactNode;
  onDrop: (draggingBlockId: string | undefined, droppedBlockId: string | undefined, dropAtEndOfList: boolean) => void;
};

export const GanttDnDHOC = function GanttDnDHOC(props: Props) {
  const { children } = props;

  return <div className="relative">{children(false)}</div>;
};
