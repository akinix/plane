// FLOW: Main CalendarView container — month grid + DragDropContext
// Adapted from Plane calendar/calendar.tsx
// Uses @hello-pangea/dnd DragDropContext, simplified local state for month navigation
import { useState, useMemo, useCallback, useRef } from "react";
import { DragDropContext, type DropResult } from "@hello-pangea/dnd";
import type { TIssue, ICalendarWeek, ICalendarPayload } from "@plane/types";
import { renderFormattedPayloadDate } from "@plane/utils";
import { CalendarHeader } from "./calendar-header";
import { CalendarWeekHeader } from "./week-header";
import { CalendarWeekDays } from "./week-days";

// Helpers for building calendar month data
function getWeekNumberOfDate(date: Date): number {
  const startDate = new Date(date.getFullYear(), 0, 1);
  const days = Math.floor((date.getTime() - startDate.getTime()) / (24 * 60 * 60 * 1000));
  return Math.ceil((days + 1) / 7);
}

function buildCalendarPayload(year: number, month: number): ICalendarPayload {
  const firstDay = new Date(year, month, 1);
  const totalDays = new Date(year, month + 1, 0).getDate();
  const startDayOfWeek = firstDay.getDay(); // 0=Sun

  const payload: ICalendarPayload = {};
  payload[`y-${year}`] ??= {};
  payload[`y-${year}`][`m-${month}`] = {};

  const numWeeks = Math.ceil((totalDays + startDayOfWeek) / 7);

  for (let week = 0; week < numWeeks; week++) {
    const currentWeek: ICalendarWeek = {};

    for (let i = 0; i < 7; i++) {
      const dayNumber = week * 7 + i - startDayOfWeek;
      const date = new Date(year, month, dayNumber + 1);
      const formattedDate = renderFormattedPayloadDate(date);
      if (formattedDate) {
        currentWeek[formattedDate] = {
          date,
          year,
          month,
          day: dayNumber + 1,
          week: getWeekNumberOfDate(date),
          is_current_month: date.getMonth() === month,
          is_current_week: getWeekNumberOfDate(date) === getWeekNumberOfDate(new Date()),
          is_today: date.toDateString() === new Date().toDateString(),
        };
      }
    }

    payload[`y-${year}`][`m-${month}`][`w-${week}`] = currentWeek;
  }

  return payload;
}

type TProps = {
  issues: TIssue[];
  isLoading: boolean;
  projectId: string;
  workspaceId: string;
  handleDragAndDrop: (
    issueId: string | undefined,
    issueProjectId: string | undefined,
    sourceDate: string | undefined,
    destinationDate: string | undefined
  ) => Promise<void>;
};

export const CalendarView = function CalendarView(props: TProps) {
  const { issues, isLoading, handleDragAndDrop } = props;

  // Current viewing month
  const [currentMonth, setCurrentMonth] = useState(() => {
    const now = new Date();
    return new Date(now.getFullYear(), now.getMonth(), 1);
  });

  // StrictMode drag guard
  const dragHandledRef = useRef(false);

  const year = currentMonth.getFullYear();
  const month = currentMonth.getMonth();

  // Build calendar payload
  const calendarPayload = useMemo(() => buildCalendarPayload(year, month), [year, month]);

  // Get weeks for the current month
  const allWeeks = useMemo(() => {
    const monthData = calendarPayload[`y-${year}`]?.[`m-${month}`];
    if (!monthData) return [];
    return Object.values(monthData) as ICalendarWeek[];
  }, [calendarPayload, year, month]);

  // Group issues by target_date
  const issuesByDate = useMemo(() => {
    const map: Record<string, TIssue[]> = {};
    for (const issue of issues) {
      if (!issue.target_date) continue;
      // Only show issues whose target_date falls in the current month range
      const issueDate = new Date(issue.target_date);
      if (issueDate.getMonth() !== month || issueDate.getFullYear() !== year) continue;

      const key = renderFormattedPayloadDate(issue.target_date);
      if (!key) continue;
      if (!map[key]) map[key] = [];
      map[key].push(issue);
    }
    return map;
  }, [issues, month, year]);

  const onPrevMonth = useCallback(() => {
    setCurrentMonth((prev) => {
      const y = prev.getMonth() === 0 ? prev.getFullYear() - 1 : prev.getFullYear();
      const m = prev.getMonth() === 0 ? 11 : prev.getMonth() - 1;
      return new Date(y, m, 1);
    });
  }, []);

  const onNextMonth = useCallback(() => {
    setCurrentMonth((prev) => {
      const y = prev.getMonth() === 11 ? prev.getFullYear() + 1 : prev.getFullYear();
      const m = (prev.getMonth() + 1) % 12;
      return new Date(y, m, 1);
    });
  }, []);

  const onToday = useCallback(() => {
    const now = new Date();
    setCurrentMonth(new Date(now.getFullYear(), now.getMonth(), 1));
  }, []);

  // Drag drop handler — threat guard T-17-CAL-01
  const onDragEnd = useCallback(
    (result: DropResult) => {
      if (dragHandledRef.current) return;
      const { draggableId, destination, source } = result;
      if (!destination) return;
      if (destination.droppableId === source.droppableId && destination.index === source.index) return;

      dragHandledRef.current = true;
      setTimeout(() => {
        dragHandledRef.current = false;
      }, 300);

      handleDragAndDrop(draggableId, undefined, source.droppableId, destination.droppableId);
    },
    [handleDragAndDrop]
  );

  // Loading state
  if (isLoading) {
    return (
      <div className="flex h-full w-full items-center justify-center">
        <div className="flex flex-col items-center gap-2">
          <div className="border-custom-border-200 border-t-custom-primary h-6 w-6 animate-spin rounded-full border-2" />
          <span className="text-sm text-custom-text-400">加载中...</span>
        </div>
      </div>
    );
  }

  // Check if there are no issues this month
  const hasIssuesThisMonth = issues.length > 0;

  return (
    <div className="flex h-full w-full flex-col overflow-hidden">
      <CalendarHeader
        currentMonth={currentMonth}
        onPrevMonth={onPrevMonth}
        onNextMonth={onNextMonth}
        onToday={onToday}
      />

      <div className="flex w-full flex-col overflow-y-auto">
        <CalendarWeekHeader showWeekends />

        <DragDropContext onDragEnd={onDragEnd}>
          <div className="h-full w-full">
            <div className="divide-custom-border-200 grid h-full w-full grid-cols-1 divide-y-[0.5px]">
              {allWeeks.map((week, weekIndex) => {
                // Use first date in week as stable key
                const weekKeys = Object.keys(week);
                const weekKey = weekKeys[0] ?? `week-${weekIndex}`;
                return (
                  <CalendarWeekDays
                    key={weekKey}
                    week={week as unknown as ICalendarWeek}
                    showWeekends
                    issuesByDate={issuesByDate}
                    handleDragAndDrop={handleDragAndDrop}
                  />
                );
              })}
            </div>
          </div>
        </DragDropContext>

        {/* Empty state */}
        {!hasIssuesThisMonth && (
          <div className="flex items-center justify-center py-16">
            <span className="text-sm text-custom-text-400">该月份没有 Issue</span>
          </div>
        )}
      </div>
    </div>
  );
};
