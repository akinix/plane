// FLOW: AnalyticsBarChart — Recharts BarChart wrapper per Plane chart type patterns (per UI-SPEC)
"use client";

import React from "react";
import { ResponsiveContainer, BarChart as RechartsBarChart, Bar, XAxis, YAxis, Tooltip, CartesianGrid } from "recharts";
import type { TChartDatum } from "@plane/types";
import { CHART_COLOR_PALETTES } from "@plane/constants";

interface AnalyticsBarChartProps {
  data: TChartDatum[];
  xKey: string;
  yKey: string;
  groupBy?: string;
  height?: number;
}

export const AnalyticsBarChart: React.FC<AnalyticsBarChartProps> = ({ data, xKey, yKey, groupBy, height = 300 }) => {
  const horizonPalette = CHART_COLOR_PALETTES.find((p) => p.key === "horizon")?.light ?? [
    "#E76E50",
    "#289D90",
    "#F3A362",
    "#E9C368",
    "#264753",
  ];

  const fillColor = groupBy ? horizonPalette[0] : horizonPalette[0];

  return (
    <ResponsiveContainer width="100%" height={height}>
      <RechartsBarChart data={data} margin={{ top: 8, right: 8, left: -16, bottom: 0 }}>
        <CartesianGrid strokeDasharray="3 3" stroke="var(--border-subtle)" vertical={false} />
        <XAxis
          dataKey={xKey}
          tick={{ fontSize: 13, fill: "var(--txt-tertiary)" }}
          tickLine={false}
          axisLine={{ stroke: "var(--border-subtle)" }}
          label={{
            value: groupBy || xKey,
            position: "insideBottomRight",
            offset: -4,
            style: { fontSize: 13, fill: "var(--txt-tertiary)" },
          }}
        />
        <YAxis
          tick={{ fontSize: 13, fill: "var(--txt-tertiary)" }}
          tickLine={false}
          axisLine={false}
          label={{
            value: "数量",
            angle: -90,
            position: "insideLeft",
            style: { fontSize: 13, fill: "var(--txt-tertiary)" },
          }}
        />
        <Tooltip
          contentStyle={{
            backgroundColor: "var(--bg-surface-1)",
            border: "1px solid var(--border-subtle)",
            borderRadius: 4,
            fontSize: 13,
          }}
          labelStyle={{ fontWeight: 600, marginBottom: 4 }}
        />
        {groupBy ? (
          // For grouped data, render a bar per unique group value
          <Bar dataKey={yKey} fill={fillColor} radius={[2, 2, 0, 0]} />
        ) : (
          <Bar dataKey={yKey} fill={fillColor} radius={[2, 2, 0, 0]} />
        )}
      </RechartsBarChart>
    </ResponsiveContainer>
  );
};
