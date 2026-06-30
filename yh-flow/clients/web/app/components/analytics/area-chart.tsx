// FLOW: AnalyticsAreaChart — Recharts AreaChart wrapper per Plane chart type patterns (per UI-SPEC)
"use client";

import React from "react";
import {
  ResponsiveContainer,
  AreaChart as RechartsAreaChart,
  Area,
  XAxis,
  YAxis,
  Tooltip,
  CartesianGrid,
} from "recharts";

interface AreaConfig {
  key: string;
  label: string;
  color: string;
}

interface AnalyticsAreaChartProps {
  data: Record<string, number | string>[];
  xKey: string;
  areas: AreaConfig[];
  height?: number;
}

export const AnalyticsAreaChart: React.FC<AnalyticsAreaChartProps> = ({ data, xKey, areas, height = 300 }) => {
  // Default colors if no areas provided: created (blue), resolved (green)
  const defaultAreas: AreaConfig[] = [
    { key: "created", label: "已创建", color: "#3B82F6" },
    { key: "resolved", label: "已完成", color: "#10B981" },
  ];
  const resolvedAreas = areas.length > 0 ? areas : defaultAreas;

  return (
    <ResponsiveContainer width="100%" height={height}>
      <RechartsAreaChart data={data} margin={{ top: 8, right: 8, left: -16, bottom: 0 }}>
        <CartesianGrid strokeDasharray="3 3" stroke="var(--border-subtle)" vertical={false} />
        <XAxis
          dataKey={xKey}
          tick={{ fontSize: 13, fill: "var(--txt-tertiary)" }}
          tickLine={false}
          axisLine={{ stroke: "var(--border-subtle)" }}
          label={{
            value: "日期",
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
        {resolvedAreas.map((area) => (
          <Area
            key={area.key}
            type="monotone"
            dataKey={area.key}
            name={area.label}
            stroke={area.color}
            fill={area.color}
            fillOpacity={0.1}
            strokeWidth={2}
          />
        ))}
      </RechartsAreaChart>
    </ResponsiveContainer>
  );
};
