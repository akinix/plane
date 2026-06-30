// FLOW: AnalyticsEmptyState — Empty state for analytics dashboard (per UI-SPEC)
"use client";

import React from "react";
import { BarChart3 } from "lucide-react";

export const AnalyticsEmptyState: React.FC = () => {
  return (
    <div className="flex h-full w-full flex-col items-center justify-center gap-3 px-6 py-16">
      <BarChart3 className="text-custom-text-300 size-12" />
      <h3 className="text-lg text-custom-text-100 font-semibold">暂无分析数据</h3>
      <p className="text-sm text-custom-text-300">选择项目后查看分析图表</p>
    </div>
  );
};
