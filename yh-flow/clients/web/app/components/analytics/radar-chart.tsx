// FLOW: AnalyticsRadarChart — Recharts RadarChart wrapper per Plane chart type patterns (per UI-SPEC)
"use client";

import React from "react";
import {
  ResponsiveContainer,
  RadarChart as RechartsRadarChart,
  Radar,
  PolarGrid,
  PolarAngleAxis,
  Tooltip,
} from "recharts";
import { CHART_COLOR_PALETTES } from "@plane/constants";
import type { TChartDatum } from "@plane/types";

interface RadarConfig {
  key: string;
  name: string;
  fill: string;
  stroke: string;
}

interface AnalyticsRadarChartProps {
  data: TChartDatum[];
  radars: RadarConfig[];
  angleKey: string;
  height?: number;
}

export const AnalyticsRadarChart: React.FC<AnalyticsRadarChartProps> = ({ data, radars, angleKey, height = 300 }) => {
  const modernPalette = CHART_COLOR_PALETTES.find((p) => p.key === "modern")?.light ?? [
    "#6172E8",
    "#8B6EDB",
    "#E05F99",
    "#29A383",
  ];

  const defaultRadars: RadarConfig[] = [
    { key: "count", name: "数量", fill: modernPalette[0], stroke: modernPalette[0] },
  ];
  const resolvedRadars = radars.length > 0 ? radars : defaultRadars;

  return (
    <ResponsiveContainer width="100%" height={height}>
      <RechartsRadarChart data={data} margin={{ top: 8, right: 8, left: 8, bottom: 8 }}>
        <PolarGrid stroke="var(--border-subtle)" />
        <PolarAngleAxis dataKey={angleKey} tick={{ fontSize: 13, fill: "var(--txt-tertiary)" }} />
        <Tooltip
          contentStyle={{
            backgroundColor: "var(--bg-surface-1)",
            border: "1px solid var(--border-subtle)",
            borderRadius: 4,
            fontSize: 13,
          }}
        />
        {resolvedRadars.map((radar) => (
          <Radar
            key={radar.key}
            dataKey={radar.key}
            name={radar.name}
            fill={radar.fill}
            stroke={radar.stroke}
            fillOpacity={0.2}
          />
        ))}
      </RechartsRadarChart>
    </ResponsiveContainer>
  );
};
