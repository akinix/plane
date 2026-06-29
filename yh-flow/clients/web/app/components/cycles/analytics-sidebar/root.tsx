// FLOW: CycleAnalyticsSidebar — analytics sidebar container (per D-P18-08)
import React from "react";
import { observer } from "mobx-react";
import { Loader } from "@plane/ui";
import { useCycleProgress } from "@/lib/hooks/use-cycle-issues";
import { CycleSidebarHeader } from "./sidebar-header";
import { CycleSidebarDetails } from "./sidebar-details";
import { CycleAnalyticsProgress } from "./issue-progress";

type Props = {
  handleClose: () => void;
  cycleId: string;
  projectId: string;
  workspaceSlug: string;
};

export const CycleDetailsSidebar = observer(function CycleDetailsSidebar(props: Props) {
  const { handleClose, projectId, cycleId } = props;
  const { data: progress, isLoading } = useCycleProgress(projectId, cycleId);

  if (isLoading)
    return (
      <Loader className="px-5">
        <div className="space-y-2">
          <Loader.Item height="15px" width="50%" />
          <Loader.Item height="15px" width="30%" />
        </div>
        <div className="mt-8 space-y-3">
          <Loader.Item height="30px" />
          <Loader.Item height="30px" />
          <Loader.Item height="30px" />
        </div>
      </Loader>
    );

  return (
    <div className="relative pb-2">
      <div className="flex w-full flex-col gap-5">
        <CycleSidebarHeader handleClose={handleClose} />
        <CycleSidebarDetails projectId={projectId} progress={progress} />
      </div>
      {projectId && cycleId && <CycleAnalyticsProgress projectId={projectId} cycleId={cycleId} />}
    </div>
  );
});
