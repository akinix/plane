// FLOW: OverviewTab — Analytics overview tab with insight cards + radar chart + active projects (per UI-SPEC)
"use client";

import React, { useMemo } from "react";
import { observer } from "mobx-react";
import { useParams } from "react-router";
import { ChartBar } from "lucide-react";
import { useAnalyticsOverview } from "@/lib/hooks/use-analytics";
import { InsightCard } from "./insight-card";
import { AnalyticsRadarChart } from "./radar-chart";
import { AnalyticsSkeleton } from "./analytics-skeleton";
import { AnalyticsEmptyState } from "./analytics-empty-state";

export const OverviewTab = observer(function OverviewTab() {
  const { workspaceId } = useParams<{ workspaceId: string }>();
  const wsId = workspaceId ?? "";
  const { data: overview, isLoading, isError, refetch } = useAnalyticsOverview(wsId);

  const cards = useMemo(() => {
    if (!overview?.data) return [];
    return overview.data.map((d) => ({
      key: d.key,
      title: d.name,
      value: d.count,
    }));
  }, [overview]);

  // Build radar chart data from overview schema keys mapped to project-like comparison
  const radarData = useMemo(() => {
    if (!overview?.data) return [];
    return overview.data.map((d) => ({
      key: d.key,
      name: d.name,
      count: d.count,
    }));
  }, [overview]);

  if (isLoading) {
    return <AnalyticsSkeleton />;
  }

  if (isError) {
    return (
      <div className="flex flex-col items-center justify-center gap-3 px-6 py-12">
        <p className="text-sm text-custom-text-300">加载分析数据失败，请重试</p>
        <button onClick={() => refetch()} className="bg-custom-primary rounded-md px-3 py-1.5 text-13 text-white">
          重试
        </button>
      </div>
    );
  }

  if (!overview?.data || overview.data.length === 0) {
    return <AnalyticsEmptyState />;
  }

  return (
    <div className="flex flex-col gap-6">
      {/* Insight Cards — 4-column grid */}
      <div className="grid grid-cols-4 gap-4 max-lg:grid-cols-2">
        {cards.map((card) => (
          <InsightCard key={card.key} title={card.title} value={card.value} />
        ))}
      </div>

      {/* Radar Chart — project comparison */}
      <div className="border-custom-border-200 bg-custom-background-90 rounded-md border p-4">
        <h3 className="text-custom-text-100 mb-4 text-18 font-semibold">项目对比</h3>
        <AnalyticsRadarChart
          data={radarData}
          radars={[{ key: "count", name: "数量", fill: "#6172E8", stroke: "#6172E8" }]}
          angleKey="name"
        />
      </div>

      {/* Active Projects List */}
      <div className="border-custom-border-200 bg-custom-background-90 rounded-md border p-4">
        <h3 className="text-custom-text-100 mb-3 text-18 font-semibold">活跃项目</h3>
        <div className="flex flex-col gap-2">
          {radarData.map((item) => {
            const total = radarData.reduce((sum, d) => sum + (typeof d.count === "number" ? d.count : 0), 0);
            const pct = total > 0 ? Math.round(((typeof item.count === "number" ? item.count : 0) / total) * 100) : 0;
            return (
              <div
                key={item.key}
                className="hover:bg-custom-background-80 flex items-center justify-between rounded-md px-3 py-2"
              >
                <div className="flex items-center gap-2">
                  <ChartBar className="text-custom-text-300 size-4" />
                  <span className="text-sm text-custom-text-200">{item.name}</span>
                </div>
                <span className="text-custom-text-300 text-13">{pct}%</span>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
});
