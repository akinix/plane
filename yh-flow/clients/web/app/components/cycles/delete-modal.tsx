// FLOW: Forked from Plane cycles/delete-modal.tsx
// FLOW: CycleDeleteModal — 删除确认弹窗
import { useState } from "react";
import { observer } from "mobx-react";
import { AlertTriangle } from "lucide-react";
import { useCycleMutations } from "@/lib/hooks/use-cycles";

interface ICycleDelete {
  cycle: any;
  isOpen: boolean;
  handleClose: () => void;
  workspaceSlug: string;
  projectId: string;
}

export const CycleDeleteModal = observer(function CycleDeleteModal(props: ICycleDelete) {
  const { isOpen, handleClose, cycle, workspaceSlug, projectId } = props;
  const [loading, setLoading] = useState(false);
  const { deleteCycle } = useCycleMutations();

  const formSubmit = async () => {
    if (!cycle?.id) return;
    setLoading(true);
    try {
      await deleteCycle.mutateAsync(cycle.id);
    } catch {
      // Error handled in mutation
    } finally {
      setLoading(false);
      handleClose();
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="fixed inset-0 bg-black/50" onClick={handleClose} />
      <div className="relative z-10 w-full max-w-md rounded-lg border border-subtle bg-surface-1 p-6 shadow-xl">
        <div className="flex items-start gap-3">
          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-danger-secondary/20">
            <AlertTriangle className="h-5 w-5 text-danger-primary" />
          </div>
          <div className="flex-1">
            <h3 className="text-16 font-semibold text-primary">删除周期</h3>
            <p className="mt-2 text-13 text-tertiary">
              确定删除此周期 <span className="font-medium text-primary">{cycle?.name}</span>？周期内的 Issue 不会被删除。
            </p>
            <p className="mt-1 text-12 text-tertiary">此操作不可撤销。</p>
          </div>
        </div>
        <div className="mt-6 flex items-center justify-end gap-2">
          <button
            className="rounded-md border border-subtle bg-surface-1 px-4 py-2 text-13 font-medium text-primary hover:bg-surface-2"
            onClick={handleClose}
          >
            取消
          </button>
          <button
            className="rounded-md bg-danger-primary px-4 py-2 text-13 font-medium text-white hover:bg-danger-primary/90"
            onClick={formSubmit}
            disabled={loading}
          >
            {loading ? "删除中..." : "删除"}
          </button>
        </div>
      </div>
    </div>
  );
});
