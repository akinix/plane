// FLOW: Simplified useTimeLineChartStore hook for yh-flow
// FLOW: Replaces Plane MobX useTimeLineChartStore with React context-based state
// FLOW: Provides essential gantt chart state management: view, blocks, drag state
import { createContext, useContext, useState, useCallback, useRef, type ReactNode } from "react";
import type { ChartDataType, IGanttBlock, IBlockUpdateDependencyData, TGanttViews } from "@plane/types";
import { currentViewDataWithView } from "../data";
import { getItemPositionWidth, getDateFromPositionOnGantt } from "../views";

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

export interface TimeLineChartStore {
  // View state
  currentView: TGanttViews;
  currentViewData: ChartDataType | undefined;
  renderView: any;

  // Block state
  blockIds: string[];
  blocksMap: Record<string, IGanttBlock & { position?: { marginLeft: number; width: number } }>;
  activeBlockId: string | null;

  // Drag state
  isDragging: boolean;

  // Actions
  setBlockIds: (ids: string[]) => void;
  updateBlockPosition: (blockId: string, deltaLeft: number, deltaWidth: number) => void;
  getBlockById: (id: string) => (IGanttBlock & { position?: { marginLeft: number; width: number } }) | undefined;
  getUpdatedPositionAfterDrag: (blockId: string, shouldUpdateHalfBlock: boolean) => IBlockUpdateDependencyData[];
  updateActiveBlockId: (id: string | null) => void;
  isBlockActive: (id: string) => boolean;
  getIsCurrentDependencyDragging: (blockId: string) => boolean;
  updateCurrentView: (view: TGanttViews) => void;
  updateCurrentViewData: (data: ChartDataType) => void;
  updateRenderView: (renderData: any) => void;
  updateAllBlocksOnChartChangeWhileDragging: (scrollWidth: number) => void;
  setCurrentView: (view: TGanttViews) => void;
  getDateFromPositionOnGantt: (position: number, offsetDays: number) => Date | undefined;
  getNumberOfDaysFromPosition: (width?: number) => number;
  setInitialBlocks: (blocks: any[], chartData: ChartDataType) => void;
  setIsDragging: (value: boolean) => void;
}

// ---------------------------------------------------------------------------
// Provider
// ---------------------------------------------------------------------------

const TimeLineChartContext = createContext<TimeLineChartStore | null>(null);

export const TimeLineChartProvider = ({ children }: { children: ReactNode }) => {
  const [currentView, setCurrentView] = useState<TGanttViews>("month");
  const [currentViewData, setCurrentViewData] = useState<ChartDataType | undefined>(() => currentViewDataWithView("month"));
  const [renderView, setRenderView] = useState<any>(null);
  const [blockIds, setBlockIds] = useState<string[]>([]);
  const [blocksMap, setBlocksMap] = useState<Record<string, IGanttBlock & { position?: { marginLeft: number; width: number } }>>({});
  const [activeBlockId, setActiveBlockId] = useState<string | null>(null);
  const [isDragging, setIsDragging] = useState(false);
  const initialPositionsRef = useRef<Record<string, { marginLeft: number; width: number }>>({});

  const updateBlockPosition = useCallback((blockId: string, deltaLeft: number, deltaWidth: number) => {
    setBlocksMap((prev) => {
      const block = prev[blockId];
      if (!block || !block.position) return prev;
      return {
        ...prev,
        [blockId]: {
          ...block,
          position: {
            marginLeft: block.position.marginLeft + deltaLeft,
            width: Math.max(block.position.width + deltaWidth, 20),
          },
        },
      };
    });
  }, []);

  const getBlockById = useCallback(
    (id: string) => blocksMap[id],
    [blocksMap]
  );

  const getUpdatedPositionAfterDrag = useCallback(
    (blockId: string, _shouldUpdateHalfBlock: boolean): IBlockUpdateDependencyData[] => {
      const block = blocksMap[blockId];
      if (!block || !block.position || !currentViewData) return [];

      const newStartDate = getDateFromPositionOnGantt(block.position.marginLeft, currentViewData, 0);
      const newEndDate = getDateFromPositionOnGantt(block.position.marginLeft + block.position.width, currentViewData, -1);

      const updates: IBlockUpdateDependencyData[] = [
        {
          id: blockId,
          start_date: newStartDate ? newStartDate.toISOString() : undefined,
          target_date: newEndDate ? newEndDate.toISOString() : undefined,
        },
      ];
      return updates;
    },
    [blocksMap, currentViewData]
  );

  const isBlockActive = useCallback((id: string) => activeBlockId === id, [activeBlockId]);

  const getIsCurrentDependencyDragging = useCallback((_blockId: string) => false, []);

  const updateCurrentView = useCallback((view: TGanttViews) => {
    setCurrentView(view);
  }, []);

  const updateCurrentViewData = useCallback((data: ChartDataType) => {
    setCurrentViewData(data);
  }, []);

  const updateRenderView = useCallback((renderData: any) => {
    setRenderView(renderData);
  }, []);

  const updateAllBlocksOnChartChangeWhileDragging = useCallback((_scrollWidth: number) => {
    // FLOW: simplified - no-op for now
  }, []);

  const _getDateFromPositionOnGantt = useCallback(
    (position: number, offsetDays: number): Date | undefined => {
      if (!currentViewData) return undefined;
      return getDateFromPositionOnGantt(position, currentViewData, offsetDays);
    },
    [currentViewData]
  );

  const getNumberOfDaysFromPosition = useCallback((width?: number): number => {
    if (!width || !currentViewData) return 0;
    return Math.round(width / currentViewData.data.dayWidth);
  }, [currentViewData]);

  const setInitialBlocks = useCallback((blocks: any[], chartData: ChartDataType) => {
    const blockIdsList: string[] = [];
    const map: Record<string, IGanttBlock & { position?: { marginLeft: number; width: number } }> = {};
    const positions: Record<string, { marginLeft: number; width: number }> = {};

    blocks.forEach((block: any) => {
      const id = block.id;
      blockIdsList.push(id);

      const position = getItemPositionWidth(chartData, {
        id,
        data: block,
        name: block.name || "",
        start_date: block.start_date,
        target_date: block.target_date,
        sort_order: block.sort_order ?? 0,
      });

      map[id] = {
        data: block,
        id,
        name: block.name || "",
        position: position ?? { marginLeft: 0, width: 0 },
        start_date: block.start_date,
        target_date: block.target_date,
        sort_order: block.sort_order ?? 0,
      };

      if (position) {
        positions[id] = position;
      }
    });

    initialPositionsRef.current = positions;
    setBlockIds(blockIdsList);
    setBlocksMap(map);
  }, []);

  const store: TimeLineChartStore = {
    currentView,
    currentViewData,
    renderView,
    blockIds,
    blocksMap,
    activeBlockId,
    isDragging,

    setBlockIds,
    updateBlockPosition,
    getBlockById,
    getUpdatedPositionAfterDrag,
    updateActiveBlockId: setActiveBlockId,
    isBlockActive,
    getIsCurrentDependencyDragging,
    updateCurrentView,
    updateCurrentViewData,
    updateRenderView,
    updateAllBlocksOnChartChangeWhileDragging,
    setCurrentView,
    getDateFromPositionOnGantt: _getDateFromPositionOnGantt,
    getNumberOfDaysFromPosition,
    setInitialBlocks,
    setIsDragging,
  };

  return (
    <TimeLineChartContext.Provider value={store}>
      {children}
    </TimeLineChartContext.Provider>
  );
};

export const useTimeLineChartStore = (): TimeLineChartStore => {
  const store = useContext(TimeLineChartContext);
  if (!store) {
    throw new Error("useTimeLineChartStore must be used within a TimeLineChartProvider");
  }
  return store;
};
