// FLOW: Forked from Plane cycles/modal.tsx
// FLOW: CycleModal — Cycle 创建/编辑弹窗
import { useStore } from "@/lib/store-context";
import { CycleForm } from "./form";

type CycleModalProps = {
  isOpen?: boolean;
  handleClose?: () => void;
  data?: any;
  workspaceSlug?: string;
  projectId?: string;
};

export function CycleModal(props: CycleModalProps) {
  const {
    isOpen: propsIsOpen,
    handleClose: propsHandleClose,
    data: propsData,
    workspaceSlug: propsWs,
    projectId: propsPid,
  } = props;
  // Use CycleStore when props not explicitly passed (standalone mode)
  const { cycle: cycleStore } = useStore();

  // Determine values: explicit props > cycle store
  const isOpen = propsIsOpen ?? cycleStore.cycleModalOpen;
  const data = propsData;
  const workspaceId = propsWs ?? "ws-1";
  const projectId = propsPid ?? "proj-1";
  const isEdit = !!data || cycleStore.cycleModalMode === "edit";

  const handleClose = () => {
    if (propsHandleClose) {
      propsHandleClose();
    } else {
      cycleStore.closeCycleModal();
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center pt-[10vh]">
      <div className="fixed inset-0 bg-black/50" onClick={handleClose} />
      <div className="relative z-10 w-full max-w-lg rounded-lg border border-subtle bg-surface-1 shadow-xl">
        <div className="flex items-center justify-between border-b border-subtle px-5 py-4">
          <h3 className="text-18 font-medium text-primary">{isEdit ? "编辑周期" : "创建周期"}</h3>
          <button className="text-tertiary hover:text-primary" onClick={handleClose}>
            <span className="text-lg">&times;</span>
          </button>
        </div>
        <CycleForm
          onClose={handleClose}
          workspaceId={workspaceId}
          projectId={projectId}
          data={data ?? null}
        />
      </div>
    </div>
  );
}
