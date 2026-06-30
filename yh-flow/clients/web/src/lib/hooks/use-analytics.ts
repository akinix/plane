// FLOW: TanStack Query hooks for workspace analytics (mock data layer per D-P20-07)
import { useCallback } from "react";
import { useQuery } from "@tanstack/react-query";
import { download, generateCsv, mkConfig } from "export-to-csv";
import type { TChart, TChartDatum } from "../../types/charts/common";
import type { TAnalyticsFilterParams } from "../../types/analytics";
import analyticsService from "../services/analytics.service";
import { MOCK_ANALYTICS_OVERVIEW } from "../mock-data";

export const useAnalyticsOverview = (workspaceId: string) => {
  return useQuery<TChart>({
    queryKey: ["analytics-overview", workspaceId],
    queryFn: async () => {
      return analyticsService.getOverview(workspaceId);
    },
    enabled: !!workspaceId,
  });
};

export const useAnalyticsTrend = (workspaceId: string) => {
  return useQuery<TChartDatum[]>({
    queryKey: ["analytics-trend", workspaceId],
    queryFn: async () => {
      return analyticsService.getTrend(workspaceId);
    },
    enabled: !!workspaceId,
  });
};

export const useAnalyticsBarData = (workspaceId: string) => {
  return useQuery<TChart>({
    queryKey: ["analytics-bar", workspaceId],
    queryFn: async () => {
      return analyticsService.getBarData(workspaceId);
    },
    enabled: !!workspaceId,
  });
};

export const useAnalyticsExport = (_workspaceId: string) => {
  return useCallback((_params?: TAnalyticsFilterParams) => {
    const csvConfig = mkConfig({
      fieldSeparator: ",",
      filename: `analytics-export-${Date.now()}`,
      decimalSeparator: ".",
      useKeysAsHeaders: true,
      showTitle: false,
    });

    const csvData = MOCK_ANALYTICS_OVERVIEW.data.map((d) => ({
      类别: d.name,
      数量: d.count,
      占比: `${Math.round((d.count / 128) * 100)}%`,
    }));

    const csv = generateCsv(csvConfig)(csvData);
    download(csvConfig)(csv);
  }, []);
};
