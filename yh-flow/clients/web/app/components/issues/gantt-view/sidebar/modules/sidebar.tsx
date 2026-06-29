// FLOW: Forked from Plane gantt-chart/sidebar/modules/sidebar.tsx
// FLOW: Removed mobx-react observer wrapper
// FLOW: Removed useTimeLineChart, GanttDnDHOC dependencies
/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// ui
import type { IBlockUpdateData } from "@plane/types";
import { Loader } from "@plane/ui";
// local imports
import { useTimeLineChartStore } from "../../hooks/use-timeline-chart";
import { ModulesSidebarBlock } from "./block";

type Props = {
  title: string;
  blockUpdateHandler: (block: any, payload: IBlockUpdateData) => void;
  blockIds: string[];
  enableReorder: boolean;
};

export const ModuleGanttSidebar = function ModuleGanttSidebar(props: Props) {
  const { blockUpdateHandler, blockIds, enableReorder } = props;

  const { getBlockById } = useTimeLineChartStore();

  return (
    <div className="h-full">
      {blockIds ? (
        blockIds.map((blockId, index) => (
          <ModulesSidebarBlock key={blockId} blockId={blockId} isDragging={false} />
        ))
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
