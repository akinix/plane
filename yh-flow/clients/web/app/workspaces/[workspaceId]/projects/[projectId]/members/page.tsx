// FLOW: Project Members page (PROJ-04)
import { useParams } from "react-router";
import { ProjectMemberList } from "../../../../../components/project/project-member-list";

export default function ProjectMembersPage() {
  const { workspaceId, projectId } = useParams<{
    workspaceId: string;
    projectId: string;
  }>();

  return (
    <div className="mx-auto flex w-full max-w-3xl flex-col gap-6 p-6">
      <h1 className="text-2xl font-semibold text-custom-text-100">项目成员</h1>
      <div className="rounded-lg border border-custom-border-200 bg-custom-background-100 px-4">
        <ProjectMemberList
          workspaceId={workspaceId ?? ""}
          projectId={projectId ?? ""}
        />
      </div>
    </div>
  );
}
