// FLOW: Forked from Plane calendar/issue-block.tsx
// Adapted: replaced Next.js router with props, removed @plane/propel/popover/toast and @plane/hooks
// Use @hello-pangea/dnd Draggable, simplified issue display
import { Draggable } from "@hello-pangea/dnd";
import type { TIssue } from "@plane/types";
import { cn } from "@plane/utils";
import { MOCK_STATES } from "@/../src/lib/mock-data";

type TProps = {
  issue: TIssue;
  index: number;
};

export const CalendarIssueBlock = function CalendarIssueBlock(props: TProps) {
  const { issue, index } = props;

  // Resolve state color
  const stateColor = MOCK_STATES.find((s) => s.id === issue.state_id)?.color ?? "#9CA3AF";

  return (
    <Draggable draggableId={issue.id} index={index}>
      {(provided, snapshot) => {
        // FLOW: entire block is the drag handle — draggableProps alone enables drag
        const draggableProps = provided.draggableProps as React.HTMLAttributes<HTMLDivElement>;
        return (
          <div
            ref={provided.innerRef as React.Ref<HTMLDivElement>}
            {...draggableProps}
            className={cn(
              "text-xs mb-0.5 flex h-7 w-full items-center gap-1.5 rounded-sm px-1.5 py-1 transition-colors",
              {
                "bg-custom-background-90 shadow-md": snapshot.isDragging,
                "bg-custom-background-100 hover:bg-custom-background-90": !snapshot.isDragging,
              }
            )}
          >
            {/* State color indicator */}
            <span className="h-full w-0.5 flex-shrink-0 rounded-sm" style={{ backgroundColor: stateColor }} />

            {/* Issue name */}
            <span className="text-custom-text-100 truncate">{issue.name}</span>
          </div>
        );
      }}
    </Draggable>
  );
};
