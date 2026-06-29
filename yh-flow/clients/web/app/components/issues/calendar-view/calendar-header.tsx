// FLOW: Forked from Plane calendar/header.tsx (renamed from header.tsx to calendar-header.tsx)
// Adapted: removed @plane/propel icons, @plane/i18n, @plane/ui Row, useCalendarView store
// Use lucide-react icons and local state for month navigation
import { ChevronLeft, ChevronRight } from "lucide-react";
import { MONTHS_LIST } from "./constants";

type TProps = {
  currentMonth: Date;
  onPrevMonth: () => void;
  onNextMonth: () => void;
  onToday: () => void;
};

export const CalendarHeader = function CalendarHeader(props: TProps) {
  const { currentMonth, onPrevMonth, onNextMonth, onToday } = props;

  const year = currentMonth.getFullYear();
  const monthIndex = currentMonth.getMonth(); // 0-indexed
  const monthShortLabel = MONTHS_LIST[monthIndex + 1]?.shortTitle ?? "";

  return (
    <div className="mb-3 flex items-center justify-between gap-2 px-1">
      <div className="flex items-center gap-1.5">
        <button
          type="button"
          onClick={onPrevMonth}
          className="text-custom-text-400 hover:bg-custom-background-80 hover:text-custom-text-200 grid place-items-center rounded-sm p-1"
        >
          <ChevronLeft className="h-4 w-4" />
        </button>
        <button
          type="button"
          onClick={onNextMonth}
          className="text-custom-text-400 hover:bg-custom-background-80 hover:text-custom-text-200 grid place-items-center rounded-sm p-1"
        >
          <ChevronRight className="h-4 w-4" />
        </button>
        <span className="text-sm text-custom-text-100 font-semibold">
          {monthShortLabel} {year}
        </span>
      </div>
      <div className="flex items-center gap-1.5">
        <button
          type="button"
          onClick={onToday}
          className="bg-custom-background-90 text-xs text-custom-text-300 hover:bg-custom-background-80 hover:text-custom-text-100 rounded-sm px-2.5 py-1 font-medium"
        >
          今天
        </button>
      </div>
    </div>
  );
};
