// FLOW: Module list page — entry point for /workspaces/:wsId/projects/:projId/modules (per MODU-01, MODU-03)
"use client";

import { observer } from "mobx-react";
import { useParams } from "react-router";
import { ModulesListView } from "@/components/modules/modules-list-view";

const ModulesPage = observer(function ModulesPage() {
  const { workspaceId, projectId } = useParams<{
    workspaceId: string;
    projectId: string;
  }>();

  if (!workspaceId || !projectId) return null;

  return (
    <div className="flex h-full flex-col">
      <ModulesListView workspaceId={workspaceId} projectId={projectId} />
    </div>
  );
});

export default ModulesPage;
