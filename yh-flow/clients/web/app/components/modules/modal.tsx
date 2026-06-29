// FLOW: Forked from Plane modules/modal.tsx
// FLOW: ModuleModal — Module 创建/编辑弹窗
import { useStore } from "@/lib/store-context";
import { ModuleForm } from "./form";

type ModuleModalProps = {
  isOpen?: boolean;
  handleClose?: () => void;
  data?: any;
  workspaceId?: string;
  projectId?: string;
};

export function ModuleModal(props: ModuleModalProps) {
  const {
    isOpen: propsIsOpen,
    handleClose: propsHandleClose,
    data: propsData,
    workspaceId: propsWs,
    projectId: propsPid,
  } = props;

  const { module: moduleStore } = useStore();

  const isOpen = propsIsOpen ?? moduleStore.moduleModalOpen ?? false;
  const data = propsData ?? moduleStore.editingModule ?? null;
  const workspaceId = propsWs ?? "ws-1";
  const projectId = propsPid ?? "proj-1";
  const isEdit = !!data || moduleStore.cycleModalMode === "edit";

  const handleClose = () => {
    if (propsHandleClose) {
      propsHandleClose();
    } else {
      moduleStore.closeModuleModal();
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center pt-[10vh]">
      <div className="fixed inset-0 bg-black/50" onClick={handleClose} />
      <div className="relative z-10 w-full max-w-lg rounded-lg border border-subtle bg-surface-1 shadow-xl">
        <div className="flex items-center justify-between border-b border-subtle px-5 py-4">
          <h3 className="text-18 font-medium text-primary">{isEdit ? "编辑模块" : "创建模块"}</h3>
          <button className="text-tertiary hover:text-primary" onClick={handleClose}>
            <span className="text-lg">&times;</span>
          </button>
        </div>
        <ModuleForm
          onClose={handleClose}
          workspaceId={workspaceId}
          projectId={projectId}
          data={data ?? null}
        />
      </div>
    </div>
  );
}
