// FLOW: Create Workspace page — standalone page, no sidebar (WORK-01, D-P15-04)
import { CreateWorkspaceForm } from "../../components/workspace/create-workspace-form";

export default function CreateWorkspacePage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-custom-background-100">
      <div className="w-full max-w-md rounded-lg border border-custom-border-200 bg-custom-background-90 p-8 shadow-lg">
        <h1 className="mb-6 text-center text-xl font-semibold text-custom-text-100">
          创建工作区
        </h1>
        <CreateWorkspaceForm />
      </div>
    </div>
  );
}
