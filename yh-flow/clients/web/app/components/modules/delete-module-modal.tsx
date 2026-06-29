// FLOW: Forked from Plane modules/delete-module-modal.tsx
// FLOW: DeleteModuleModal — Module 删除确认弹窗
import { useState } from "react";
import { useModuleMutations } from "@/../src/lib/hooks/use-modules";

type Props = {
  moduleId: string;
  moduleName: string;
  isOpen: boolean;
  onClose: () => void;
};

export function DeleteModuleModal(props: Props) {
  const { moduleId, moduleName, isOpen, onClose } = props;
  const [isDeleting, setIsDeleting] = useState(false);
  const { deleteModule } = useModuleMutations();

  const handleDelete = async () => {
    setIsDeleting(true);
    try {
      await deleteModule.mutateAsync(moduleId);
      onClose();
    } catch {
      // Error handled by mutation
    } finally {
      setIsDeleting(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="fixed inset-0 bg-black/50" onClick={onClose} />
      <div className="relative z-10 w-full max-w-md rounded-lg border border-subtle bg-surface-1 p-6 shadow-xl">
        <h3 className="text-18 font-medium text-primary">删除模块</h3>
        <p className="mt-2 text-14 text-secondary">
          确定删除此模块？模块内的 Issue 不会被删除。
        </p>
        {moduleName && (
          <p className="mt-1 text-14 font-medium text-primary">{moduleName}</p>
        )}
        <div className="mt-6 flex items-center justify-end gap-2">
          <button
            onClick={onClose}
            className="rounded-md border border-subtle px-4 py-2 text-13 text-primary hover:bg-surface-1"
          >
            取消
          </button>
          <button
            onClick={handleDelete}
            disabled={isDeleting}
            className="rounded-md bg-danger-primary px-4 py-2 text-13 text-white hover:bg-danger-primary/90 disabled:opacity-50"
          >
            {isDeleting ? "删除中..." : "删除"}
          </button>
        </div>
      </div>
    </div>
  );
}
