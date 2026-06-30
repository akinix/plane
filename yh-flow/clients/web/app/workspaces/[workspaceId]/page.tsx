// FLOW: Workspace Dashboard — project overview grid + recent activity list (WORK-01, D-P15-09)
import { useParams } from "react-router";
import { useWorkspace } from "../../../src/lib/hooks/use-workspaces";
import { MOCK_ACTIVITIES } from "../../../src/lib/mock-data";
import { ProjectGrid } from "../../components/workspace/dashboard/project-grid";
import { ActivityList } from "../../components/workspace/dashboard/activity-list";

export default function WorkspaceDashboardPage() {
  const { workspaceId } = useParams<{ workspaceId: string }>();
  const { data: workspace, isLoading } = useWorkspace(workspaceId ?? "");

  // Filter activities for this workspace
  const activities = MOCK_ACTIVITIES.filter((a) => a.workspace === workspaceId);

  if (isLoading) {
    return (
      <div className="flex h-full items-center justify-center">
        <div className="border-custom-border-strong border-t-custom-primary size-8 animate-spin rounded-full border-2" />
      </div>
    );
  }

  return (
    <div className="mx-auto flex w-full max-w-6xl flex-col gap-6 overflow-x-hidden p-4 md:p-6">
      {/* Workspace header */}
      <div>
        <h1 className="text-2xl text-custom-text-100 font-semibold">{workspace?.name ?? "工作区"}</h1>
        {workspace?.slug && <p className="text-sm text-custom-text-300">{workspace.slug}</p>}
      </div>

      {/* Project overview grid */}
      <section>
        <h2 className="text-sm text-custom-text-200 mb-3 font-medium">项目概览</h2>
        <ProjectGrid workspaceId={workspaceId ?? ""} />
      </section>

      {/* Recent activity */}
      <section>
        <h2 className="text-sm text-custom-text-200 mb-3 font-medium">最近活动</h2>
        <div className="border-custom-border-200 bg-custom-background-100 rounded-lg border px-4">
          <ActivityList activities={activities} />
        </div>
      </section>
    </div>
  );
}
