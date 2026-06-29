// FLOW: KanbanCard — single draggable card in a kanban column (per D-P16-05)
import { useCallback } from "react";
import { useNavigate } from "react-router";
import { observer } from "mobx-react";
import { Draggable } from "@hello-pangea/dnd";
import { GripVertical } from "lucide-react";
import { cn } from "@plane/utils";
import { Avatar } from "@plane/ui";
import { MOCK_LABELS } from "@/../src/lib/mock-data";
import type { TIssue } from "@plane/types";

type TProps = {
  issue: TIssue;
  index: number;
  columnId: string;
  workspaceId: string;
  projectId: string;
  projectIdentifier: string;
};

const PRIORITY_CONFIG: Record<string, { label: string; className: string }> = {
  urgent: { label: "紧急", className: "text-color-priority-urgent border-color-priority-urgent bg-red-500/10" },
  high: { label: "高", className: "text-color-priority-high border-color-priority-high bg-orange-500/10" },
  medium: { label: "中", className: "text-color-priority-medium border-color-priority-medium bg-yellow-500/10" },
  low: { label: "低", className: "text-color-priority-low border-color-priority-low bg-blue-500/10" },
  none: { label: "无", className: "text-color-priority-none border-color-priority-none bg-gray-500/10" },
};

const ASSIGNEE_NAMES: Record<string, string> = {
  "user-1": "张三",
  "user-2": "李四",
  "user-3": "王五",
  "user-4": "赵六",
  "user-5": "陈七",
};

const KanbanCard = observer(function KanbanCard({
  issue,
  index,
  columnId,
  workspaceId,
  projectId,
  projectIdentifier,
}: TProps) {
  const navigate = useNavigate();

  const handleClick = useCallback(() => {
    navigate(`/workspaces/${workspaceId}/projects/${projectId}/issues/${issue.id}`);
  }, [navigate, workspaceId, projectId, issue.id]);

  const priority = issue.priority ?? "none";
  const priorityCfg = PRIORITY_CONFIG[priority] ?? PRIORITY_CONFIG.none;

  // Get assignee info
  const firstAssigneeId = issue.assignee_ids[0];

  // Get labels (max 2)
  const issueLabels = issue.label_ids
    .slice(0, 2)
    .map((lid) => MOCK_LABELS.find((l) => l.id === lid))
    .filter(Boolean);
  const remainingLabels = Math.max(0, issue.label_ids.length - 2);

  return (
    <Draggable draggableId={issue.id} index={index} key={issue.id}>
      {(provided, snapshot) => (
        <div
          ref={provided.innerRef}
          {...provided.draggableProps}
          className={cn(
            "mb-2 rounded-md bg-custom-background-100 p-3 shadow-sm transition-shadow",
            "hover:shadow-md hover:ring-1 hover:ring-custom-primary/20",
            snapshot.isDragging && "shadow-lg ring-2 ring-custom-primary/30 rotate-2"
          )}
          style={{
            ...provided.draggableProps.style,
            minHeight: "80px",
          }}
        >
          {/* Drag handle */}
          <div {...provided.dragHandleProps} className="mb-1.5 flex items-center text-custom-text-400">
            <GripVertical className="size-3.5" />
          </div>

          {/* Click area — navigates to detail page */}
          <div
            onClick={handleClick}
            role="button"
            tabIndex={0}
            onKeyDown={(e) => {
              if (e.key === "Enter") handleClick();
            }}
            className="cursor-pointer"
          >
            {/* Issue title — 2-line truncation */}
            <h4 className="line-clamp-2 text-sm font-medium text-custom-text-100">{issue.name}</h4>

            {/* Issue ID — project identifier + sequence_id */}
            <p className="mt-1 text-xs text-custom-text-400">
              {projectIdentifier}-{issue.sequence_id}
            </p>

            {/* Priority badge + Assignee row */}
            <div className="mt-2 flex items-center gap-2">
              <span
                className={cn(
                  "inline-flex items-center rounded-sm border px-1.5 py-0.5 text-[10px] font-medium",
                  priorityCfg.className
                )}
              >
                {priorityCfg.label}
              </span>

              {firstAssigneeId && (
                <Avatar
                  name={ASSIGNEE_NAMES[firstAssigneeId] ?? "用户"}
                  size="sm"
                  showTooltip={true}
                />
              )}
            </div>

            {/* Labels row — max 2 labels + overflow count */}
            {issueLabels.length > 0 && (
              <div className="mt-2 flex flex-wrap items-center gap-1">
                {issueLabels.map(
                  (label) =>
                    label && (
                      <span
                        key={label.id}
                        className="inline-flex items-center rounded-sm px-1.5 py-0.5 text-[10px] text-custom-text-300"
                        style={{ backgroundColor: `${label.color}20` }}
                      >
                        {label.name}
                      </span>
                    )
                )}
                {remainingLabels > 0 && (
                  <span className="text-[10px] text-custom-text-400">+{remainingLabels}</span>
                )}
              </div>
            )}

            {/* Estimate */}
            {issue.estimate_point && (
              <div className="mt-2 text-[10px] text-custom-text-400">{issue.estimate_point}点</div>
            )}
          </div>
        </div>
      )}
    </Draggable>
  );
});

export { KanbanCard };
