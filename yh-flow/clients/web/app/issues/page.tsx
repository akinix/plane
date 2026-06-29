// FLOW: Issue list page — entry point for /workspaces/:wsId/projects/:projId/issues (per D-P16-01)
import { useState } from "react";
import { useParams, useNavigate } from "react-router";
import { observer } from "mobx-react";
import { List, Kanban, Plus } from "lucide-react";
import { cn } from "@plane/utils";
import { useStore } from "@/lib/store-context";
import { IssueListView } from "@/components/issues/list-view";
import { IssueCreateModal } from "@/components/issues/issue-create-modal";

const IssuesPage = observer(function IssuesPage() {
  const { workspaceId, projectId } = useParams<{ workspaceId: string; projectId: string }>();
  const navigate = useNavigate();
  const store = useStore();
  const [createModalOpen, setCreateModalOpen] = useState(false);

  if (!workspaceId || !projectId) {
    return null;
  }

  return (
    <div className="flex h-full flex-col">
      {/* Page header */}
      <div className="flex items-center justify-between border-b border-custom-border-200 px-6 py-3">
        <div className="flex items-center gap-3">
          <h1 className="text-lg font-semibold text-custom-text-100">Issues</h1>

          {/* View toggle */}
          <div className="flex items-center gap-0.5 rounded-md border border-custom-border-200 bg-custom-background-90 p-0.5">
            <button
              type="button"
              onClick={() => store.issue.setActiveView("list")}
              className={cn(
                "flex items-center gap-1 rounded-sm px-2 py-1 text-xs transition-colors",
                store.issue.activeView === "list"
                  ? "bg-custom-background-100 text-custom-text-100 shadow-sm"
                  : "text-custom-text-400 hover:text-custom-text-200"
              )}
            >
              <List className="size-3.5" />
              列表
            </button>
            <button
              type="button"
              disabled
              className="flex cursor-not-allowed items-center gap-1 rounded-sm px-2 py-1 text-xs text-custom-text-400 opacity-50"
              title="看板视图将在后续版本中启用"
            >
              <Kanban className="size-3.5" />
              看板
            </button>
          </div>
        </div>

        {/* Create Issue button */}
        <button
          type="button"
          onClick={() => setCreateModalOpen(true)}
          className="flex items-center gap-1.5 rounded-md bg-custom-primary px-3 py-2 text-sm font-medium text-white hover:bg-custom-primary/90 transition-colors"
        >
          <Plus className="size-4" />
          创建 Issue
        </button>
      </div>

      {/* Content area */}
      <div className="flex-1 overflow-auto">
        {store.issue.activeView === "list" && (
          <IssueListView workspaceId={workspaceId} projectId={projectId} />
        )}
      </div>

      {/* Create Issue modal */}
      <IssueCreateModal
        isOpen={createModalOpen}
        onClose={() => setCreateModalOpen(false)}
        workspaceId={workspaceId}
        projectId={projectId}
      />
    </div>
  );
});

export default IssuesPage;
