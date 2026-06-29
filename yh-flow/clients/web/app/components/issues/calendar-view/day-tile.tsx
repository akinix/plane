// FLOW: Forked from Plane calendar/day-tile.tsx
// Adapted: replaced @atlaskit/pragmatic-drag-and-drop with @hello-pangea/dnd Droppable
// Removed Plane store type dependencies, use@plane/types ICalendarDate
import type { TIssue, ICalendarDate } from "@plane/types";
import { renderFormattedPayloadDate } from "@plane/utils";
import { Droppable } from "@hello-pangea/dnd";
import { CalendarIssueBlocks } from "./issue-blocks";
import { MONTHS_LIST } from "./constants";

type TProps = {
  date: ICalendarDate;
  issues?: TIssue[];
  handleDragAndDrop: (
    issueId: string | undefined,
    issueProjectId: string | undefined,
    sourceDate: string | undefined,
    destinationDate: string | undefined
  ) => Promise<void>;
};

export const CalendarDayTile = function CalendarDayTile(props: TProps) {
  const { date, issues, handleDragAndDrop } = props;

  const formattedDatePayload = renderFormattedPayloadDate(date.date);
  const isToday = date.date.toDateString() === new Date().toDateString();
  const isWeekend = [0, 6].includes(date.date.getDay());

  if (!formattedDatePayload) return null;

  return (
    <div className="group border-custom-border-200 relative flex h-full w-full flex-col border-b">
      {/* Day number header */}
      <div
        className={`flex flex-shrink-0 justify-end px-1.5 py-1 text-right text-[11px] ${
          date.is_current_month ? "text-custom-text-100 font-medium" : "text-custom-text-400"
        } ${isWeekend ? "bg-custom-background-80" : "bg-transparent"}`}
      >
        {date.date.getDate() === 1 && MONTHS_LIST[date.date.getMonth() + 1]?.shortTitle + " "}
        {isToday ? (
          <span className="bg-custom-primary text-xs flex h-5 w-5 items-center justify-center rounded-full text-white">
            {date.date.getDate()}
          </span>
        ) : (
          <span>{date.date.getDate()}</span>
        )}
      </div>

      {/* Content area with Droppable */}
      <div className="hidden h-full w-full md:block">
        <Droppable droppableId={formattedDatePayload} isDropDisabled={false}>
          {(provided, snapshot) => (
            <div
              ref={provided.innerRef}
              {...provided.droppableProps}
              className={`h-full min-h-[5rem] w-full select-none ${
                snapshot.isDraggingOver ? "bg-custom-background-80 opacity-70" : ""
              }`}
            >
              <CalendarIssueBlocks
                date={date.date}
                formattedDatePayload={formattedDatePayload}
                issues={issues}
                handleDragAndDrop={handleDragAndDrop}
              />
              {provided.placeholder}
            </div>
          )}
        </Droppable>
      </div>
    </div>
  );
};
