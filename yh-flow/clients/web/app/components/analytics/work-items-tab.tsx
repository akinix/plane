// FLOW: WorkItemsTab — Analytics work items tab with area chart, bar chart, data table (per UI-SPEC)
"use client";

import React, { useMemo } from "react";
import { observer } from "mobx-react";
import { useParams } from "react-router";
import { useAnalyticsTrend, useAnalyticsBarData } from "@/lib/hooks/use-analytics";
import { AnalyticsAreaChart } from "./area-chart";
import { AnalyticsBarChart } from "./bar-chart";
import { DataTable } from "./data-table";
import { AnalyticsSkeleton } from "./analytics-skeleton";
import { AnalyticsEmptyState } from "./analytics-empty-state";

export const WorkItemsTab = observer(function WorkItemsTab() {
  const { workspaceId } = useParams<{ workspaceId: string }>();
  const wsId = workspaceId ?? "";

  const {
    data: trendData,
    isLoading: trendLoading,
    isError: trendError,
    refetch: refetchTrend,
  } = useAnalyticsTrend(wsId);

  const { data: barData, isLoading: barLoading, isError: barError, refetch: refetchBar } = useAnalyticsBarData(wsId);

  const isLoading = trendLoading || barLoading;
  const isError = trendError || barError;

  // Prepare area chart data from trend
  const areaChartData = useMemo(() => {
    if (!trendData) return [];
    return trendData.map((d) => ({
      key: d.key,
      name: d.name,
      created: d.created ?? 0,
      resolved: d.resolved ?? 0,
    }));
  }, [trendData]);

  // Prepare bar chart data from bar mock
  const barChartData = useMemo(() => {
    if (!barData?.data) return [];
    return barData.data;
  }, [barData]);

  if (isLoading) {
    return <AnalyticsSkeleton />;
  }

  if (isError) {
    return (
      <div className="flex flex-col items-center justify-center gap-3 px-6 py-12">
        <p className="text-sm text-custom-text-300">加载分析数据失败，请重试</p>
        <button
          onClick={() => {
            refetchTrend();
            refetchBar();
          }}
          className="bg-custom-primary rounded-md px-3 py-1.5 text-13 text-white"
        >
          重试
        </button>
      </div>
    );
  }

  if ((!trendData || trendData.length === 0) && (!barData?.data || barData.data.length === 0)) {
    return <AnalyticsEmptyState />;
  }

  return (
    <div className="flex flex-col gap-6">
      {/* Area Chart — Created vs Resolved trend */}
      <div className="border-custom-border-200 bg-custom-background-90 rounded-md border p-4">
        <h3 className="text-custom-text-100 mb-4 text-18 font-semibold">创建 vs 完成趋势</h3>
        {areaChartData.length > 0 ? (
          <AnalyticsAreaChart data={areaChartData} xKey="name" areas={[]} />
        ) : (
          <p className="text-custom-text-300 py-8 text-center text-13">暂无趋势数据</p>
        )}
      </div>

      {/* Bar Chart — Priority distribution */}
      <div className="border-custom-border-200 bg-custom-background-90 rounded-md border p-4">
        <h3 className="text-custom-text-100 mb-4 text-18 font-semibold">优先级分布</h3>
        {barChartData.length > 0 ? (
          <AnalyticsBarChart data={barChartData} xKey="name" yKey="count" />
        ) : (
          <p className="text-custom-text-300 py-8 text-center text-13">暂无分布数据</p>
        )}
      </div>

      {/* Data Table */}
      {barChartData.length > 0 && (
        <DataTable
          data={barChartData}
          columns={[
            { key: "name", label: "类别" },
            { key: "count", label: "数量", sortable: true },
            { key: "percentage", label: "占比" },
          ]}
          total={barChartData.reduce((sum, d) => sum + (typeof d.count === "number" ? d.count : 0), 0)}
        />
      )}
    </div>
  );
});
