// FLOW: Forked from Plane cycles/transfer-issues-modal.tsx
// FLOW: CycleTransferModal — Issue 转移弹窗
import { useState } from "react";
import { observer } from "mobx-react";
import { Search, AlertCircle, X } from "lucide-react";
import { useTransferCycleIssues } from "@/lib/hooks/use-cycle-issues";
import { useCycles } from "@/lib/hooks/use-cycles";

type Props = {
  isOpen: boolean;
  handleClose: () => void;
  fromCycleId: string;
  projectId: string;
};

export const CycleTransferModal = observer(function CycleTransferModal(props: Props) {
  const { isOpen, handleClose, fromCycleId, projectId } = props;
  const [query, setQuery] = useState("");
  const transfer = useTransferCycleIssues();
  const { data: cycles } = useCycles(projectId);

  // Exclude current cycle, only show cycles that can receive issues
  const targetCycles = (cycles ?? []).filter((c) => c.id !== fromCycleId && c.status !== "completed");

  const filteredCycles = targetCycles.filter((c) =>
    c.name?.toLowerCase().includes(query.toLowerCase())
  );

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center pt-[10vh]">
      <div className="fixed inset-0 bg-black/50" onClick={handleClose} />
      <div className="relative z-10 w-full max-w-lg rounded-lg border border-subtle bg-surface-1 shadow-xl">
        <div className="flex items-center justify-between px-5 py-4">
          <div className="flex items-center gap-2">
            <h4 className="text-18 font-medium text-primary">转移 Issue</h4>
          </div>
          <button onClick={handleClose}>
            <X className="h-4 w-4" />
          </button>
        </div>
        <div className="flex items-center gap-2 border-b border-subtle px-5 pb-3">
          <Search className="h-4 w-4 text-secondary" />
          <input
            className="w-full border-none bg-transparent text-13 text-primary outline-none placeholder:text-placeholder"
            placeholder="搜索目标周期..."
            onChange={(e) => setQuery(e.target.value)}
            value={query}
          />
        </div>
        <div className="flex w-full flex-col items-start gap-2 px-5 py-3">
          {filteredCycles.length > 0 ? (
            filteredCycles.map((cycle) => (
              <button
                key={cycle.id}
                className="flex w-full items-center gap-3 rounded-sm px-4 py-3 text-13 text-secondary hover:bg-surface-2"
                onClick={() => {
                  transfer.mutate({ fromCycleId, toCycleId: cycle.id });
                  handleClose();
                }}
              >
                <div className="flex w-full justify-between truncate">
                  <span className="truncate">{cycle.name}</span>
                  {cycle.status && (
                    <span className="flex shrink-0 items-center rounded-full bg-layer-1 px-2 text-12 capitalize">
                      {cycle.status === "current" ? "活跃" : cycle.status === "upcoming" ? "即将开始" : "草稿"}
                    </span>
                  )}
                </div>
              </button>
            ))
          ) : (
            <div className="flex w-full items-center justify-center gap-2 p-5 text-13 text-tertiary">
              <AlertCircle className="h-3.5 w-3.5" />
              <span>没有可用的目标周期，请先创建新周期。</span>
            </div>
          )}
        </div>
      </div>
    </div>
  );
});
