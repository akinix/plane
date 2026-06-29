// FLOW: Bulk action bar — floating bar for batch operations (per D-P16-07 note)
import { useState } from "react";
import { observer } from "mobx-react";
import { Trash2, UserCheck, ArrowRight } from "lucide-react";
import { useIssueMutations } from "@/../src/lib/hooks/use-issues";
import { MOCK_MEMBERS, MOCK_STATES } from "@/../src/lib/mock-data";
import { AlertModalCore, EModalWidth } from "@plane/ui";

type TProps = {
  workspaceId: string;
  projectId: string;
  selectedIds: string[];
  onClearSelection: () => void;
};

export const BulkActionBar = observer(function BulkActionBar({
  workspaceId,
  projectId,
  selectedIds,
  onClearSelection,
}: TProps) {
  const { bulkUpdateIssues } = useIssueMutations();
  const [isDeleting, setIsDeleting] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [actionInProgress, setActionInProgress] = useState(false);

  const projectStates = MOCK_STATES.filter((s) => s.project_id === projectId);
  const workspaceMembers = MOCK_MEMBERS[workspaceId] ?? [];

  const handleStateChange = async (newStateId: string) => {
    if (!newStateId) return;
    setActionInProgress(true);
    try {
      await bulkUpdateIssues.mutateAsync({
        issue_ids: selectedIds,
        properties: { state_id: newStateId },
      });
      onClearSelection();
    } catch {
      // mock layer — noop
    } finally {
      setActionInProgress(false);
    }
  };

  const handleAssign = async (assigneeId: string) => {
    if (!assigneeId) return;
    setActionInProgress(true);
    try {
      await bulkUpdateIssues.mutateAsync({
        issue_ids: selectedIds,
        properties: { assignee_ids: [assigneeId] },
      });
      onClearSelection();
    } catch {
      // mock layer — noop
    } finally {
      setActionInProgress(false);
    }
  };

  const handleDelete = async () => {
    setActionInProgress(true);
    setIsDeleting(true);
    try {
      await bulkUpdateIssues.mutateAsync({
        issue_ids: selectedIds,
        properties: { archived_at: new Date().toISOString() },
      });
      onClearSelection();
    } catch {
      // mock layer — noop
    } finally {
      setIsDeleting(false);
      setActionInProgress(false);
      setShowDeleteConfirm(false);
    }
  };

  if (selectedIds.length === 0) return null;

  return (
    <>
      <div className="fixed bottom-6 left-1/2 z-20 -translate-x-1/2">
        <div className="flex items-center gap-3 rounded-lg border border-custom-border-200 bg-custom-background-100 px-4 py-3 shadow-lg">
          {/* Selection count */}
          <span className="text-sm font-medium text-custom-text-200">
            已选择 <span className="text-custom-text-100">{selectedIds.length}</span> 项
          </span>

          <div className="h-5 w-px bg-custom-border-200" />

          {/* State change */}
          <div className="relative">
            <select
              onChange={(e) => {
                const val = e.target.value;
                e.target.value = "";
                if (val) handleStateChange(val);
              }}
              defaultValue=""
              className="appearance-none rounded-md border border-custom-border-200 bg-custom-background-80 px-3 py-1.5 pr-8 text-xs text-custom-text-200 outline-none focus:border-custom-primary"
              disabled={actionInProgress}
            >
              <option value="" disabled>
                状态变更
              </option>
              {projectStates.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.name}
                </option>
              ))}
            </select>
            <ArrowRight className="pointer-events-none absolute right-2 top-1/2 size-3 -translate-y-1/2 text-custom-text-400" />
          </div>

          {/* Assign */}
          <div className="relative">
            <select
              onChange={(e) => {
                const val = e.target.value;
                e.target.value = "";
                if (val) handleAssign(val);
              }}
              defaultValue=""
              className="appearance-none rounded-md border border-custom-border-200 bg-custom-background-80 px-3 py-1.5 pr-8 text-xs text-custom-text-200 outline-none focus:border-custom-primary"
              disabled={actionInProgress}
            >
              <option value="" disabled>
                指派
              </option>
              {workspaceMembers.map((m) => (
                <option key={m.member.id} value={m.member.id}>
                  {m.member.display_name}
                </option>
              ))}
            </select>
            <UserCheck className="pointer-events-none absolute right-2 top-1/2 size-3 -translate-y-1/2 text-custom-text-400" />
          </div>

          {/* Delete */}
          <button
            type="button"
            onClick={() => setShowDeleteConfirm(true)}
            disabled={actionInProgress}
            className="flex items-center gap-1 rounded-md border border-red-500/30 px-3 py-1.5 text-xs text-red-500 hover:bg-red-500/10"
          >
            <Trash2 className="size-3" />
            删除
          </button>

          {/* Clear selection */}
          <button
            type="button"
            onClick={onClearSelection}
            className="text-xs text-custom-text-400 hover:text-custom-text-200"
          >
            取消选择
          </button>
        </div>
      </div>

      {/* Delete confirmation dialog */}
      <AlertModalCore
        isOpen={showDeleteConfirm}
        handleClose={() => setShowDeleteConfirm(false)}
        handleSubmit={handleDelete}
        isSubmitting={isDeleting}
        title="确认删除"
        content={`确定删除选中的 ${selectedIds.length} 个 Issue 吗？此操作将 Issue 归档，后续可以恢复。`}
        variant="danger"
        width={EModalWidth.XL}
        primaryButtonText={{
          loading: "删除中...",
          default: "确认删除",
        }}
        secondaryButtonText="取消"
      />
    </>
  );
});
