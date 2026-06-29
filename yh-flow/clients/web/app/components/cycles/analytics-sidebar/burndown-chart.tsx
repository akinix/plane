// FLOW: Burndown SVG chart per D-P18-08 | Forked from Plane cycles/analytics-sidebar/
import React from "react";

type BurndownDataPoint = {
  date: string;
  ideal: number;
  actual: number;
};

type Props = {
  data: BurndownDataPoint[];
  startDate: string;
  endDate: string;
  totalIssues: number;
};

export const BurndownChart: React.FC<Props> = ({ data, startDate, endDate, totalIssues }) => {
  const WIDTH = 400;
  const HEIGHT = 200;
  const PAD = { top: 20, right: 12, bottom: 28, left: 36 };
  const chartW = WIDTH - PAD.left - PAD.right;
  const chartH = HEIGHT - PAD.top - PAD.bottom;

  // Calculate total days in sprint
  const start = new Date(startDate);
  const end = new Date(endDate);
  const totalDays = Math.max(1, Math.ceil((end.getTime() - start.getTime()) / 86400000));
  const displayedDays = data.length > 0 ? data.length : totalDays;

  // Grid lines: 5 horizontal + 6 vertical
  const yTicks = 5;
  const xTicks = Math.min(7, displayedDays);

  const toX = (dayIdx: number) => PAD.left + (dayIdx / Math.max(1, displayedDays - 1)) * chartW;
  const toY = (value: number) => PAD.top + (1 - value / Math.max(1, totalIssues)) * chartH;

  // Build the ideal line (diagonal from top-left to bottom-right)
  const idealLine = `M${toX(0)},${toY(totalIssues)} L${toX(displayedDays - 1)},${toY(0)}`;

  // Build the actual line from data points
  const actualPath =
    data.length > 0
      ? data
          .map((p, i) => `${i === 0 ? "M" : "L"}${toX(i)},${toY(p.actual)}`)
          .join(" ")
      : idealLine;

  return (
    <div className="flex flex-col gap-2">
      <h3 className="text-xs font-medium text-secondary">Burndown 图</h3>
      <svg viewBox={`0 0 ${WIDTH} ${HEIGHT}`} className="w-full h-auto" role="img" aria-label="Burndown chart">
        {/* Grid background - horizontal lines */}
        {Array.from({ length: yTicks + 1 }).map((_, i) => {
          const y = PAD.top + (i / yTicks) * chartH;
          const val = Math.round(totalIssues - (i / yTicks) * totalIssues);
          return (
            <React.Fragment key={`h-${i}`}>
              <line x1={PAD.left} y1={y} x2={WIDTH - PAD.right} y2={y} stroke="oklch(0.7 0 0 / 0.2)" strokeWidth={1} strokeDasharray="4 4" />
              <text x={PAD.left - 4} y={y + 3} textAnchor="end" className="fill-tertiary" fontSize={10}>
                {val}
              </text>
            </React.Fragment>
          );
        })}

        {/* Grid background - vertical lines */}
        {Array.from({ length: xTicks }).map((_, i) => {
          const dayIdx = Math.round((i / Math.max(1, xTicks - 1)) * (displayedDays - 1));
          const x = toX(dayIdx);
          const d = new Date(start.getTime() + dayIdx * 86400000);
          const label = `${d.getMonth() + 1}/${d.getDate()}`;
          return (
            <text key={`v-${i}`} x={x} y={HEIGHT - PAD.bottom + 14} textAnchor="middle" className="fill-tertiary" fontSize={10}>
              {label}
            </text>
          );
        })}

        {/* Ideal line (dashed) */}
        <path d={idealLine} fill="none" stroke="oklch(0.6 0 0 / 0.4)" strokeWidth={1.5} strokeDasharray="6 3" />

        {/* Actual line (solid accent) */}
        <path d={actualPath} fill="none" stroke="oklch(0.55 0.15 250)" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round" />

        {/* Data dots on actual line */}
        {data.map((p, i) => (
          <circle key={`dot-${i}`} cx={toX(i)} cy={toY(p.actual)} r={2.5} fill="oklch(0.55 0.15 250)" />
        ))}
      </svg>
    </div>
  );
};
