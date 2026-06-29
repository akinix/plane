// FLOW: Forked from Plane cycles/transfer-issues.tsx
// FLOW: TransferIssues — Issue 转移逻辑组件（不含 UI，供 delete-modal 复用）
import { AlertCircle } from "lucide-react";

type Props = {
  handleClick: () => void;
  canTransferIssues?: boolean;
  disabled?: boolean;
};

export function TransferIssues(props: Props) {
  const { handleClick, canTransferIssues = false, disabled = false } = props;

  return (
    <div className="mb-4 flex items-center justify-between px-4 pt-6">
      <div className="flex items-center gap-2 text-13 text-secondary">
        <AlertCircle className="h-3.5 w-3.5" />
        <span>已完成的周期不可编辑。</span>
      </div>
      {canTransferIssues && (
        <button
          className="flex items-center gap-1.5 rounded-md bg-accent-primary px-3 py-1.5 text-13 font-medium text-white hover:bg-accent-primary/90 disabled:opacity-50"
          onClick={handleClick}
          disabled={disabled}
        >
          转移 Issue
        </button>
      )}
    </div>
  );
}
