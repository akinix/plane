// FLOW: Forked from Plane cycles/quick-actions.tsx
// FLOW: CycleQuickActions — Cycle 快速操作菜单
import { useState } from "react";
import { observer } from "mobx-react";
import { MoreHorizontal } from "lucide-react";
import { useStore } from "@/lib/store-context";
import { useCycleMutations } from "@/lib/hooks/use-cycles";
import { CycleDeleteModal } from "./delete-modal";
import { CycleModal as CycleCreateUpdateModal } from "./modal";

type Props = {
  parentRef: React.RefObject<HTMLElement>;
  cycleId: string;
  projectId: string;
  workspaceSlug: string;
  customClassName?: string;
};

export const CycleQuickActions = observer(function CycleQuickActions(props: Props) {
  const { parentRef, cycleId, projectId, workspaceSlug, customClassName } = props;
  const [updateModal, setUpdateModal] = useState(false);
  const [deleteModal, setDeleteModal] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);

  const handleCopyLink = () => {
    const cycleLink = `${workspaceSlug}/projects/${projectId}/cycles/${cycleId}`;
    navigator.clipboard.writeText(cycleLink);
    setMenuOpen(false);
  };

  return (
    <>
      {updateModal && (
        <CycleCreateUpdateModal
          data={null}
          isOpen={updateModal}
          handleClose={() => setUpdateModal(false)}
          workspaceSlug={workspaceSlug}
          projectId={projectId}
        />
      )}
      {deleteModal && (
        <CycleDeleteModal
          cycle={null as any}
          isOpen={deleteModal}
          handleClose={() => setDeleteModal(false)}
          workspaceSlug={workspaceSlug}
          projectId={projectId}
        />
      )}
      <div className="relative">
        <button
          className="flex items-center justify-center rounded-md p-1 text-tertiary hover:bg-surface-2"
          onClick={(e) => {
            e.preventDefault();
            e.stopPropagation();
            setMenuOpen(!menuOpen);
          }}
        >
          <MoreHorizontal className="h-4 w-4" />
        </button>
        {menuOpen && (
          <>
            <div className="fixed inset-0 z-10" onClick={() => setMenuOpen(false)} />
            <div className="absolute right-0 z-20 min-w-40 rounded-md border border-subtle bg-surface-1 py-1 shadow-lg">
              <button
                className="flex w-full items-center px-3 py-1.5 text-13 text-primary hover:bg-surface-2"
                onClick={() => {
                  setMenuOpen(false);
                  setUpdateModal(true);
                }}
              >
                编辑
              </button>
              <button
                className="flex w-full items-center px-3 py-1.5 text-13 text-primary hover:bg-surface-2"
                onClick={() => {
                  setMenuOpen(false);
                  handleCopyLink();
                }}
              >
                复制链接
              </button>
              <hr className="my-1 border-subtle" />
              <button
                className="flex w-full items-center px-3 py-1.5 text-13 text-danger-primary hover:bg-surface-2"
                onClick={() => {
                  setMenuOpen(false);
                  setDeleteModal(true);
                }}
              >
                删除
              </button>
            </div>
          </>
        )}
      </div>
    </>
  );
});
