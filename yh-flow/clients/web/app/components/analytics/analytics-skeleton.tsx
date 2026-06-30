// FLOW: AnalyticsSkeleton — Loading skeleton for analytics dashboard (per UI-SPEC)
"use client";

import React from "react";
import { Loader } from "@plane/ui";

const CARD_SKELETONS = ["card-1", "card-2", "card-3", "card-4"];
const CHART_SKELETONS = ["chart-area", "chart-bar"];

export const AnalyticsSkeleton: React.FC = () => {
  return (
    <div className="flex flex-col gap-6">
      {/* 4 Card skeleton items in grid */}
      <div className="grid grid-cols-4 gap-4 max-lg:grid-cols-2">
        {CARD_SKELETONS.map((id) => (
          <div key={id} className="h-24 w-full">
            <Loader>
              <Loader.Item height="h-full" width="w-full" />
            </Loader>
          </div>
        ))}
      </div>

      {/* 2 Chart skeleton items */}
      {CHART_SKELETONS.map((id) => (
        <div key={id} className="h-80 w-full">
          <Loader>
            <Loader.Item height="h-full" width="w-full" />
          </Loader>
        </div>
      ))}
    </div>
  );
};
