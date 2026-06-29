// FLOW: Forked from Plane calendar/week-days.tsx
// Adapted: simplified props, removed Plane store types, uses ICalendarDate/ICalendarWeek from @plane/types
import type { TIssue, ICalendarWeek } from "@plane/types";
import { cn, renderFormattedPayloadDate } from "@plane/utils";
import { CalendarDayTile } from "./day-tile";

type TProps = {
  week: ICalendarWeek | undefined;
  showWeekends?: boolean;
  issuesByDate?: Record<string, TIssue[]>;
  handleDragAndDrop: (
    issueId: string | undefined,
    issueProjectId: string | undefined,
    sourceDate: string | undefined,
    destinationDate: string | undefined
  ) => Promise<void>;
};

export const CalendarWeekDays = function CalendarWeekDays(props: TProps) {
  const { week, showWeekends = true, issuesByDate, handleDragAndDrop } = props;

  if (!week) return null;

  const shouldShowDay = (dayDate: Date) => {
    if (showWeekends) return true;
    const day = dayDate.getDay();
    return !(day === 0 || day === 6);
  };

  const weekEntries = Object.values(week);

  return (
    <div
      className={cn("divide-custom-border-200 grid divide-x-[0.5px]", {
        "grid-cols-7": showWeekends,
        "grid-cols-5": !showWeekends,
      })}
    >
      {weekEntries.map((date) => {
        if (!shouldShowDay(date.date)) return null;
        const dateKey = renderFormattedPayloadDate(date.date) ?? "";
        const dayIssues = issuesByDate?.[dateKey];

        return <CalendarDayTile key={dateKey} date={date} issues={dayIssues} handleDragAndDrop={handleDragAndDrop} />;
      })}
    </div>
  );
};
