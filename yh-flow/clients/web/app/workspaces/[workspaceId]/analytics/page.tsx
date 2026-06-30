// FLOW: Analytics dashboard page — workspace-level analysis with tabs, charts, filter bar (per D-P20-05)
"use client";

import React, { useCallback } from "react";
import { useParams } from "react-router";
import { observer } from "mobx-react";
import { useStore } from "@/lib/store-context";
import { useProjects } from "@/lib/hooks/use-projects";
import { useAnalyticsOverview, useAnalyticsTrend, useAnalyticsBarData } from "@/lib/hooks/use-analytics";
import { OverviewTab, WorkItemsTab, FilterBar } from "@/components/analytics";
import { ContentWrapper } from "@/lib/ui/content-wrapper";

const AnalyticsPage = observer(function AnalyticsPage() {
  const { workspaceId } = useParams<{ workspaceId: string }>();
  const wsId = workspaceId ?? "";
  const { analytics } = useStore();
  const { activeTab, setActiveTab } = analytics;

  // Fetch projects for filter dropdown
  const { data: projects } = useProjects(wsId);

  // Fetch analytics data for refresh
  const { refetch: refetchOverview, isFetching: overviewFetching } = useAnalyticsOverview(wsId);
  const { refetch: refetchTrend, isFetching: trendFetching } = useAnalyticsTrend(wsId);
  const { refetch: refetchBar, isFetching: barFetching } = useAnalyticsBarData(wsId);

  const isRefreshing = overviewFetching || trendFetching || barFetching;

  const handleRefresh = useCallback(() => {
    refetchOverview();
    refetchTrend();
    refetchBar();
  }, [refetchOverview, refetchTrend, refetchBar]);

  const tabs = [
    { key: "overview" as const, label: "概览" },
    { key: "work-items" as const, label: "工作项" },
  ];

  return (
    <ContentWrapper>
      <div className="mx-auto max-w-6xl">
        {/* Header */}
        <div className="border-custom-border-200 flex items-center justify-between border-b px-6 py-4">
          <h1 className="text-2xl text-custom-text-100 font-semibold">分析</h1>
          <FilterBar projects={projects ?? []} onRefresh={handleRefresh} isRefreshing={isRefreshing} />
        </div>

        {/* Tabs */}
        <div className="border-custom-border-200 flex items-center border-b px-6">
          {tabs.map((tab) => (
            <button
              key={tab.key}
              onClick={() => setActiveTab(tab.key)}
              className={`px-4 py-3 text-13 font-medium transition-colors ${
                activeTab === tab.key
                  ? "border-custom-primary text-custom-text-100 border-b-2"
                  : "text-custom-text-300 hover:text-custom-text-200"
              }`}
            >
              {tab.label}
            </button>
          ))}
        </div>

        {/* Content */}
        <div className="px-6 py-6">{activeTab === "overview" ? <OverviewTab /> : <WorkItemsTab />}</div>
      </div>
    </ContentWrapper>
  );
});

export default AnalyticsPage;
