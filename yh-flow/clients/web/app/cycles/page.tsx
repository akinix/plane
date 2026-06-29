// FLOW: Cycle list page — entry point for /workspaces/:wsId/projects/:projId/cycles (per CYCLE-01, CYCLE-04)
"use client";

import { observer } from "mobx-react";
import { useParams } from "react-router";
import { CyclesView } from "@/components/cycles/cycles-view";

const CyclesPage = observer(function CyclesPage() {
  const { workspaceId, projectId } = useParams<{
    workspaceId: string;
    projectId: string;
  }>();

  if (!workspaceId || !projectId) return null;

  return (
    <div className="flex h-full flex-col">
      <CyclesView workspaceSlug={workspaceId} projectId={projectId} />
    </div>
  );
});

export default CyclesPage;
