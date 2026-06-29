// FLOW: Project Dashboard page (PROJ-03, D-P15-11)
import { useParams } from "react-router";
import { ProjectDashboard } from "../../../../components/project/project-dashboard";

export default function ProjectDashboardPage() {
  const { workspaceId, projectId } = useParams<{
    workspaceId: string;
    projectId: string;
  }>();

  return (
    <ProjectDashboard
      workspaceId={workspaceId ?? ""}
      projectId={projectId ?? ""}
    />
  );
}
