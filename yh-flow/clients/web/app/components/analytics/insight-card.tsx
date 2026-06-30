// FLOW: InsightCard — Stat card with title, count value, optional trend indicator (per UI-SPEC)
"use client";

import React from "react";
import { TrendingUp, TrendingDown, Minus } from "lucide-react";
import { Card } from "@plane/ui";

type TrendDirection = "up" | "down" | "neutral";

interface InsightCardProps {
  title: string;
  value: number | string;
  icon?: React.ReactNode;
  trend?: {
    direction: TrendDirection;
    percentage: number;
  };
}

const trendConfig: Record<TrendDirection, { icon: React.ReactNode; color: string }> = {
  up: { icon: <TrendingUp className="size-3.5" />, color: "text-emerald-500" },
  down: { icon: <TrendingDown className="size-3.5" />, color: "text-red-500" },
  neutral: { icon: <Minus className="size-3.5" />, color: "text-custom-text-300" },
};

export const InsightCard: React.FC<InsightCardProps> = ({ title, value, icon, trend }) => {
  return (
    <Card>
      <div className="flex flex-col gap-1.5 p-4">
        <div className="flex items-center justify-between">
          <span className="text-custom-text-300 text-13">{title}</span>
          {icon && <span className="text-custom-text-300">{icon}</span>}
        </div>
        <div className="flex items-center gap-2">
          <span className="text-2xl text-custom-text-100 font-semibold">{value}</span>
          {trend && (
            <div className={`flex items-center gap-0.5 text-13 ${trendConfig[trend.direction].color}`}>
              {trendConfig[trend.direction].icon}
              <span>{trend.percentage}%</span>
            </div>
          )}
        </div>
      </div>
    </Card>
  );
};
