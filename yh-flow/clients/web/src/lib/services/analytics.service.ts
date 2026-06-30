// FLOW: AnalyticsService — Mock data service for workspace analytics (per D-P20-07)
import type { TChart, TChartDatum } from "../../types/charts/common";
import { MOCK_ANALYTICS_OVERVIEW, MOCK_ANALYTICS_TREND, MOCK_ANALYTICS_BAR } from "../mock-data";

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

class AnalyticsService {
  async getOverview(_workspaceId: string): Promise<TChart> {
    await delay(300);
    return MOCK_ANALYTICS_OVERVIEW;
  }

  async getTrend(_workspaceId: string): Promise<TChartDatum[]> {
    await delay(300);
    return MOCK_ANALYTICS_TREND;
  }

  async getBarData(_workspaceId: string): Promise<TChart> {
    await delay(300);
    return MOCK_ANALYTICS_BAR;
  }
}

const analyticsService = new AnalyticsService();
export default analyticsService;
