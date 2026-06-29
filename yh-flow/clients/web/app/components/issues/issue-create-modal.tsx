// FLOW: Issue create modal — slide-over panel for creating new Issues (per D-P16-01)
import { useState } from "react";
import { useNavigate } from "react-router";
import { observer } from "mobx-react";
import { X } from "lucide-react";
import { ModalCore } from "@plane/ui";
import { useIssueMutations } from "@/../src/lib/hooks/use-issues";
import { MOCK_MEMBERS, MOCK_LABELS, MOCK_STATES } from "@/../src/lib/mock-data";
import type { TIssuePriorities } from "@plane/types";

type TProps = {
  isOpen: boolean;
  onClose: () => void;
  workspaceId: string;
  projectId: string;
};

const PRIORITY_OPTIONS: { value: TIssuePriorities | ""; label: string; color: string }[] = [
  { value: "", label: "无", color: "#A3A3A3" },
  { value: "urgent", label: "紧急", color: "#EF4444" },
  { value: "high", label: "高", color: "#F59E0B" },
  { value: "medium", label: "中", color: "#3B82F6" },
  { value: "low", label: "低", color: "#6B7280" },
];

export const IssueCreateModal = observer(function IssueCreateModal({
  isOpen,
  onClose,
  workspaceId,
  projectId,
}: TProps) {
  const navigate = useNavigate();
  const { createIssue } = useIssueMutations();

  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [priority, setPriority] = useState<TIssuePriorities | "">("");
  const [assigneeId, setAssigneeId] = useState("");
  const [selectedLabelIds, setSelectedLabelIds] = useState<string[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const projectStates = MOCK_STATES.filter((s) => s.project_id === projectId);
  const defaultStateId = projectStates.find((s) => s.default)?.id ?? projectStates[0]?.id ?? "";
  const workspaceMembers = MOCK_MEMBERS[workspaceId] ?? [];
  const projectLabels = MOCK_LABELS.filter((l) => l.project_id === projectId);

  const resetForm = () => {
    setTitle("");
    setDescription("");
    setPriority("");
    setAssigneeId("");
    setSelectedLabelIds([]);
  };

  const handleSubmit = async () => {
    if (!title.trim()) return;

    setIsSubmitting(true);
    try {
      const result = await createIssue.mutateAsync({
        name: title.trim(),
        description_html: description ? `<p>${description}</p>` : "",
        project_id: projectId,
        state_id: defaultStateId,
        priority: priority || null,
        assignee_ids: assigneeId ? [assigneeId] : [],
        label_ids: selectedLabelIds,
      } as any);

      resetForm();
      onClose();
      navigate(`/workspaces/${workspaceId}/projects/${projectId}/issues/${result.id}`);
    } catch {
      // Error handling — mock layer should not fail
    } finally {
      setIsSubmitting(false);
    }
  };

  const toggleLabel = (labelId: string) => {
    setSelectedLabelIds((prev) =>
      prev.includes(labelId) ? prev.filter((id) => id !== labelId) : [...prev, labelId]
    );
  };

  return (
    <ModalCore isOpen={isOpen} handleClose={onClose}>
      <div className="flex max-h-[80vh] flex-col">
        {/* Header */}
        <div className="flex items-center justify-between border-b border-custom-border-200 px-5 py-4">
          <h2 className="text-lg font-semibold text-custom-text-100">创建 Issue</h2>
          <button
            type="button"
            onClick={onClose}
            className="rounded-md p-1 text-custom-text-400 hover:bg-custom-background-80 hover:text-custom-text-200"
          >
            <X className="size-4" />
          </button>
        </div>

        {/* Form */}
        <div className="flex-1 overflow-y-auto px-5 py-4">
          <div className="flex flex-col gap-4">
            {/* Title */}
            <div className="flex flex-col gap-1">
              <label className="text-xs font-medium text-custom-text-300">
                Issue 标题 <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="Issue 标题"
                className="w-full rounded-md border border-custom-border-200 bg-custom-background-90 px-3 py-2 text-sm text-custom-text-100 outline-none placeholder:text-custom-text-400 focus:border-custom-primary"
                autoFocus
              />
            </div>

            {/* Description */}
            <div className="flex flex-col gap-1">
              <label className="text-xs font-medium text-custom-text-300">描述</label>
              {/* FLOW: Stub — LiteTextEditorWithRef from @plane/editor will be used when API layer is ready */}
              <textarea
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="添加描述..."
                rows={4}
                className="w-full resize-none rounded-md border border-custom-border-200 bg-custom-background-90 px-3 py-2 text-sm text-custom-text-100 outline-none placeholder:text-custom-text-400 focus:border-custom-primary"
              />
              <p className="text-xs text-custom-text-400">
                TipTap 富文本编辑器将在 API 集成阶段启用
              </p>
            </div>

            {/* Priority */}
            <div className="flex flex-col gap-1">
              <label className="text-xs font-medium text-custom-text-300">优先级</label>
              <select
                value={priority}
                onChange={(e) => setPriority(e.target.value as TIssuePriorities | "")}
                className="w-full rounded-md border border-custom-border-200 bg-custom-background-90 px-3 py-2 text-sm text-custom-text-100 outline-none focus:border-custom-primary"
              >
                {PRIORITY_OPTIONS.map((opt) => (
                  <option key={opt.value} value={opt.value}>
                    {opt.label}
                  </option>
                ))}
              </select>
            </div>

            {/* Assignee */}
            <div className="flex flex-col gap-1">
              <label className="text-xs font-medium text-custom-text-300">负责人</label>
              <select
                value={assigneeId}
                onChange={(e) => setAssigneeId(e.target.value)}
                className="w-full rounded-md border border-custom-border-200 bg-custom-background-90 px-3 py-2 text-sm text-custom-text-100 outline-none focus:border-custom-primary"
              >
                <option value="">未指派</option>
                {workspaceMembers.map((m) => (
                  <option key={m.member.id} value={m.member.id}>
                    {m.member.display_name}
                  </option>
                ))}
              </select>
            </div>

            {/* Labels */}
            <div className="flex flex-col gap-1">
              <label className="text-xs font-medium text-custom-text-300">标签</label>
              <div className="flex flex-wrap gap-2">
                {projectLabels.length === 0 && (
                  <span className="text-xs text-custom-text-400">该项目暂无标签</span>
                )}
                {projectLabels.map((label) => (
                  <button
                    key={label.id}
                    type="button"
                    onClick={() => toggleLabel(label.id)}
                    className={`inline-flex items-center gap-1.5 rounded-full px-3 py-1 text-xs transition-colors ${
                      selectedLabelIds.includes(label.id)
                        ? "bg-custom-primary/10 text-custom-primary ring-1 ring-custom-primary"
                        : "border border-custom-border-200 text-custom-text-300 hover:bg-custom-background-80"
                    }`}
                  >
                    <span
                      className="size-2 rounded-full"
                      style={{ backgroundColor: label.color }}
                    />
                    {label.name}
                  </button>
                ))}
              </div>
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="flex items-center justify-end gap-2 border-t border-custom-border-200 px-5 py-4">
          <button
            type="button"
            onClick={() => {
              resetForm();
              onClose();
            }}
            className="rounded-md border border-custom-border-200 px-4 py-2 text-sm text-custom-text-300 hover:bg-custom-background-80"
          >
            取消
          </button>
          <button
            type="button"
            onClick={handleSubmit}
            disabled={!title.trim() || isSubmitting}
            className="flex items-center gap-1.5 rounded-md bg-custom-primary px-4 py-2 text-sm font-medium text-white hover:bg-custom-primary/90 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {isSubmitting ? (
              <>
                <span className="size-3.5 animate-spin rounded-full border-2 border-white/30 border-t-white" />
                创建中...
              </>
            ) : (
              "创建"
            )}
          </button>
        </div>
      </div>
    </ModalCore>
  );
});
