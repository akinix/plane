// FLOW: Workspace Settings page (WORK-03)
import { useParams } from "react-router";
import { WorkspaceSettingsForm } from "../../../components/workspace/settings/workspace-settings-form";

export default function WorkspaceSettingsPage() {
  const { workspaceId } = useParams<{ workspaceId: string }>();

  return (
    <div className="mx-auto flex w-full max-w-3xl flex-col gap-6 p-4 md:p-6">
      <h1 className="text-2xl font-semibold text-custom-text-100">工作区设置</h1>
      <div className="rounded-lg border border-custom-border-200 bg-custom-background-100 p-6">
        <WorkspaceSettingsForm workspaceId={workspaceId ?? ""} />
      </div>
    </div>
  );
}
