// FLOW: KanbanColumn — single droppable column in a kanban board (per D-P16-01)
import { observer } from "mobx-react";
import { Droppable } from "@hello-pangea/dnd";
import { ChevronDown, ChevronRight } from "lucide-react";
import { cn } from "@plane/utils";
import { KanbanCard } from "./kanban-card";
import type { TIssue } from "@plane/types";

type TProps = {
  columnId: string;
  title: string;
  color: string;
  issues: TIssue[];
  isExpanded: boolean;
  onToggleExpand: () => void;
  index: number;
  workspaceId: string;
  projectId: string;
  projectIdentifier: string;
};

const KanbanColumn = observer(function KanbanColumn({
  columnId,
  title,
  color,
  issues,
  isExpanded,
  onToggleExpand,
  index,
  workspaceId,
  projectId,
  projectIdentifier,
}: TProps) {
  return (
    <Droppable droppableId={columnId}>
      {(provided, snapshot) => (
        <div
          ref={provided.innerRef}
          {...provided.droppableProps}
          className={cn(
            "flex w-[280px] shrink-0 flex-col rounded-lg bg-custom-background-80 transition-colors",
            snapshot.isDraggingOver && "bg-custom-background-90 ring-2 ring-custom-primary/20"
          )}
          style={{ minHeight: "80px" }}
        >
          {/* Column header — sticky with bg-layer-3 per UI-SPEC.md */}
          <div className="sticky top-0 z-10 flex items-center justify-between rounded-t-lg bg-custom-background-90 px-3 py-2.5">
            <div className="flex items-center gap-2 min-w-0">
              {/* State color dot */}
              <span
                className="inline-block size-2.5 shrink-0 rounded-full"
                style={{ backgroundColor: color }}
              />
              {/* Column title */}
              <span className="truncate text-xs font-medium text-custom-text-200">{title}</span>
              {/* Issue count */}
              <span className="inline-flex size-5 shrink-0 items-center justify-center rounded-full bg-custom-background-80 text-[10px] text-custom-text-400">
                {issues.length}
              </span>
            </div>

            {/* Collapse/expand button */}
            <button
              type="button"
              onClick={onToggleExpand}
              className="flex size-5 shrink-0 items-center justify-center rounded text-custom-text-400 hover:bg-custom-background-80 hover:text-custom-text-200 transition-colors"
              aria-label={isExpanded ? "折叠列" : "展开列"}
            >
              {isExpanded ? <ChevronDown className="size-3.5" /> : <ChevronRight className="size-3.5" />}
            </button>
          </div>

          {/* Column body */}
          {isExpanded && (
            <div className="flex flex-col gap-0 px-2 pb-2 pt-1">
              {/* Card list */}
              {issues.map((issue, idx) => (
                <KanbanCard
                  key={issue.id}
                  issue={issue}
                  index={idx}
                  columnId={columnId}
                  workspaceId={workspaceId}
                  projectId={projectId}
                  projectIdentifier={projectIdentifier}
                />
              ))}

              {/* Empty column state */}
              {issues.length === 0 && !snapshot.isDraggingOver && (
                <div className="flex items-center justify-center py-8">
                  <p className="text-xs text-custom-text-400">拖动 Issue 到此列</p>
                </div>
              )}

              {/* Drop placeholder */}
              {provided.placeholder}
            </div>
          )}

          {/* Collapsed column shows just count */}
          {!isExpanded && (
            <div className="flex items-center justify-center py-8">
              <p className="text-[10px] text-custom-text-400">{issues.length} 个 Issue（已折叠）</p>
            </div>
          )}
        </div>
      )}
    </Droppable>
  );
});

export { KanbanColumn };
