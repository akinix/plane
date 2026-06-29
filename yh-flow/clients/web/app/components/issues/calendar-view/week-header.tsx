// FLOW: Forked from Plane calendar/week-header.tsx
// Adapted: removed useUserProfile dependency, Chinese day labels, simplified props
import { DAY_SHORT_LABELS } from "./constants";

type TProps = {
  showWeekends?: boolean;
};

export const CalendarWeekHeader = function CalendarWeekHeader({ showWeekends = true }: TProps) {
  const days = DAY_SHORT_LABELS;

  return (
    <div
      className={`divide-custom-border-200 text-xs relative grid divide-x-[0.5px] font-medium ${
        showWeekends ? "grid-cols-7" : "grid-cols-5"
      }`}
    >
      {days.map((label, index) => {
        // Hide weekends if showWeekends is false
        if (!showWeekends && (index === 0 || index === 6)) return null;

        return (
          <div key={label} className="bg-custom-background-90 flex h-9 items-center justify-center px-2 md:justify-end">
            {label}
          </div>
        );
      })}
    </div>
  );
};
