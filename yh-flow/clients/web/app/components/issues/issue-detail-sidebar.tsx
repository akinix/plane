// FLOW: IssueDetailSidebar — right column property panel per D-P16-09
// Uses PropertyEditor sub-components per D-P16-10
"use client";

import { useState } from "react";
import { observer } from "mobx-react";
import type { TIssue } from "@plane/types";
import { DeleteIssueModal } from "./delete-issue-modal";
import {
  PropertyEditorState,
  PropertyEditorPriority,
  PropertyEditorAssignee,
  PropertyEditorLabels,
  PropertyEditorEstimate,
  PropertyEditorDate,
} from "./property-editor";
import { useNavigate } from "react-router";
import { useCycles } from "@/../src/lib/hooks/use-cycles";
import { useModules } from "@/../src/lib/hooks/use-modules";

type Props = {
  issue: TIssue;
  workspaceId: string;
  projectId: string;
  onUpdate: (data: Partial<TIssue>) => void;
};

export const IssueDetailSidebar = observer(function IssueDetailSidebar({
  issue,
  workspaceId,
  projectId,
  onUpdate,
}: Props) {
  const navigate = useNavigate();
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const { data: cycles } = useCycles(projectId);
  const { data: modules } = useModules(projectId);
  const assignedCycle = cycles?.find((c) => c.id === issue.cycle_id);
  const assignedModules = modules?.filter((m) => issue.module_ids?.includes(m.id));

  const handleDelete = () => {
    navigate(`/workspaces/${workspaceId}/projects/${projectId}/issues`);
  };

  return (
    <div className="border-custom-border-200 bg-custom-background-90 flex flex-col gap-4 rounded-md border p-4">
      <h3 className="text-sm text-custom-text-200 font-semibold">属性</h3>

      <div className="flex flex-col gap-3">
        {/* State */}
        <div className="flex flex-col gap-1">
          <span className="text-xs text-custom-text-400">状态</span>
          <PropertyEditorState
            value={issue.state_id}
            projectId={projectId}
            onSelect={(stateId) => onUpdate({ state_id: stateId })}
          />
        </div>

        {/* Priority */}
        <div className="flex flex-col gap-1">
          <span className="text-xs text-custom-text-400">优先级</span>
          <PropertyEditorPriority value={issue.priority} onSelect={(priority) => onUpdate({ priority })} />
        </div>

        {/* Assignee */}
        <div className="flex flex-col gap-1">
          <span className="text-xs text-custom-text-400">负责人</span>
          <PropertyEditorAssignee
            value={issue.assignee_ids}
            workspaceId={workspaceId}
            onSelect={(assigneeIds) => onUpdate({ assignee_ids: assigneeIds })}
          />
        </div>

        {/* Labels */}
        <div className="flex flex-col gap-1">
          <span className="text-xs text-custom-text-400">标签</span>
          <PropertyEditorLabels
            value={issue.label_ids}
            projectId={projectId}
            onSelect={(labelIds) => onUpdate({ label_ids: labelIds })}
          />
        </div>

        {/* Estimate */}
        <div className="flex flex-col gap-1">
          <span className="text-xs text-custom-text-400">估算</span>
          <PropertyEditorEstimate
            value={issue.estimate_point}
            onSelect={(estimate) => onUpdate({ estimate_point: estimate })}
          />
        </div>

        {/* Due Date */}
        <div className="flex flex-col gap-1">
          <span className="text-xs text-custom-text-400">截止日期</span>
          <PropertyEditorDate value={issue.target_date} onSelect={(date) => onUpdate({ target_date: date })} />
        </div>

        {/* Cycle — 显示关联周期名称，可点击进入周期详情 */}
        {issue.cycle_id && assignedCycle && (
          <div className="flex flex-col gap-1">
            <span className="text-xs text-custom-text-400">周期</span>
            <button
              onClick={() =>
                navigate(`/workspaces/${workspaceId}/projects/${projectId}/cycles/${issue.cycle_id}`)
              }
              className="text-sm text-custom-primary-100 hover:underline text-left transition-colors"
            >
              {assignedCycle.name}
            </button>
          </div>
        )}

        {/* Module — 显示关联模块名称列表，每个可点击进入模块详情 */}
        {assignedModules && assignedModules.length > 0 && (
          <div className="flex flex-col gap-1">
            <span className="text-xs text-custom-text-400">模块</span>
            <div className="flex flex-wrap gap-1">
              {assignedModules.map((mod) => (
                <button
                  key={mod.id}
                  onClick={() =>
                    navigate(`/workspaces/${workspaceId}/projects/${projectId}/modules/${mod.id}`)
                  }
                  className="text-sm text-custom-primary-100 hover:underline text-left transition-colors"
                >
                  {mod.name}
                </button>
              ))}
            </div>
          </div>
        )}
      </div>

      {/* Delete button */}
      <hr className="border-custom-border-200" />
      <button
        onClick={() => setDeleteModalOpen(true)}
        className="text-sm flex w-full items-center gap-2 rounded-md px-2 py-1.5 text-danger-primary transition-colors hover:bg-danger-subtle"
      >
        删除 Issue
      </button>

      <DeleteIssueModal
        isOpen={deleteModalOpen}
        onClose={() => setDeleteModalOpen(false)}
        issueId={issue.id}
        issueName={issue.name}
        projectId={projectId}
        onDelete={handleDelete}
      />
    </div>
  );
});
