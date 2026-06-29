// FLOW: Forked from Plane gantt-chart/sidebar/utils.ts
/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

import type { ChartDataType, IBlockUpdateData, IGanttBlock } from "@plane/types";

export const handleOrderChange = (
  draggingBlockId: string | undefined,
  droppedBlockId: string | undefined,
  dropAtEndOfList: boolean,
  blockIds: string[] | null,
  getBlockById: (id: string, currentViewData?: ChartDataType) => IGanttBlock,
  blockUpdateHandler: (block: any, payload: IBlockUpdateData) => void
) => {
  if (!blockIds || !draggingBlockId || !droppedBlockId) return;

  const sourceBlockIndex = blockIds.findIndex((id) => id === draggingBlockId);
  const destinationBlockIndex = dropAtEndOfList ? blockIds.length : blockIds.findIndex((id) => id === droppedBlockId);

  if (sourceBlockIndex === -1 || destinationBlockIndex === -1 || sourceBlockIndex === destinationBlockIndex) return;

  let updatedSortOrder = getBlockById(blockIds[sourceBlockIndex])?.sort_order ?? 0;

  if (destinationBlockIndex === 0) updatedSortOrder = (getBlockById(blockIds[0])?.sort_order ?? 0) - 1000;
  else if (destinationBlockIndex === blockIds.length)
    updatedSortOrder = (getBlockById(blockIds[blockIds.length - 1])?.sort_order ?? 0) + 1000;
  else {
    const destinationSortingOrder = getBlockById(blockIds[destinationBlockIndex])?.sort_order ?? 0;
    const relativeDestinationSortingOrder = getBlockById(blockIds[destinationBlockIndex - 1])?.sort_order ?? 0;
    updatedSortOrder = (destinationSortingOrder + relativeDestinationSortingOrder) / 2;
  }

  blockUpdateHandler(getBlockById(blockIds[sourceBlockIndex])?.data, {
    sort_order: {
      destinationIndex: destinationBlockIndex,
      newSortOrder: updatedSortOrder,
      sourceIndex: sourceBlockIndex,
    },
  });
};
