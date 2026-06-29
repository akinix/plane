// FLOW: Project Settings page (PROJ-03)
import { useParams } from "react-router";
import { ProjectSettingsForm } from "../../../../../components/project/project-settings-form";

export default function ProjectSettingsPage() {
  const { workspaceId, projectId } = useParams<{
    workspaceId: string;
    projectId: string;
  }>();

  return (
    <div className="mx-auto flex w-full max-w-3xl flex-col gap-6 p-6">
      <h1 className="text-2xl font-semibold text-custom-text-100">项目设置</h1>
      <div className="rounded-lg border border-custom-border-200 bg-custom-background-100 p-6">
        <ProjectSettingsForm
          workspaceId={workspaceId ?? ""}
          projectId={projectId ?? ""}
        />
      </div>
    </div>
  );
}
