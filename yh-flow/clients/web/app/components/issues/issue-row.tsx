// FLOW: Issue row component for list view (per D-P16-05)
import { useNavigate } from "react-router";
import { formatDistanceToNow } from "date-fns";
import { zhCN } from "date-fns/locale";
import { cn } from "@plane/utils";
import { Avatar } from "@plane/ui";
import type { TIssue } from "@plane/types";
import { MOCK_STATES, MOCK_MEMBERS } from "@/../src/lib/mock-data";

type TProps = {
  issue: TIssue;
  workspaceId: string;
  projectId: string;
  isSelected: boolean;
  onToggleSelect: () => void;
};

const PRIORITY_STYLES: Record<string, string> = {
  urgent: "text-color-priority-urgent",
  high: "text-color-priority-high",
  medium: "text-color-priority-medium",
  low: "text-color-priority-low",
  none: "text-color-priority-none",
};

const PRIORITY_LABELS: Record<string, string> = {
  urgent: "紧急",
  high: "高",
  medium: "中",
  low: "低",
  none: "无",
};

export const IssueRow = ({ issue, workspaceId, projectId, isSelected, onToggleSelect }: TProps) => {
  const navigate = useNavigate();

  const state = MOCK_STATES.find((s) => s.id === issue.state_id);
  const assignee = issue.assignee_ids[0]
    ? MOCK_MEMBERS[workspaceId]?.find((m) => m.member.id === issue.assignee_ids[0])
    : undefined;

  const handleRowClick = () => {
    navigate(`/workspaces/${workspaceId}/projects/${projectId}/issues/${issue.id}`);
  };

  const handleCheckboxClick = (e: React.MouseEvent) => {
    e.stopPropagation();
    onToggleSelect();
  };

  const projectKey = issue.project_id
    ? MOCK_STATES.find((s) => s.project_id === issue.project_id)?.project_id === issue.project_id
      ? (MOCK_STATES.find((s) => s.project_id === issue.project_id)?.name ?? "")
      : ""
    : "";

  return (
    <div
      onClick={handleRowClick}
      className={cn(
        "flex h-11 cursor-pointer items-center gap-3 border-b border-custom-border-200 px-4 text-sm transition-colors hover:bg-custom-background-80",
        isSelected && "bg-custom-primary/5"
      )}
    >
      {/* Checkbox */}
      <div className="flex shrink-0 items-center" onClick={handleCheckboxClick}>
        <input
          type="checkbox"
          checked={isSelected}
          onChange={() => {}}
          className="size-4 rounded border-custom-border-300 text-custom-primary focus:ring-custom-primary"
        />
      </div>

      {/* Issue ID */}
      <span className="w-20 shrink-0 text-xs text-custom-text-400 font-mono">
        {issue.project_id ? `${MOCK_STATES.find((s) => s.project_id === issue.project_id && (s.id === issue.state_id || true))?.id ?? ""}` : ""}
        {issue.sequence_id}
      </span>

      {/* Title */}
      <span className="min-w-0 flex-1 truncate text-custom-text-100">
        {issue.name}
      </span>

      {/* Priority badge */}
      <span className={cn("w-12 shrink-0 text-xs font-medium", PRIORITY_STYLES[issue.priority ?? "none"] ?? "text-custom-text-400")}>
        {PRIORITY_LABELS[issue.priority ?? "none"] ?? "无"}
      </span>

      {/* Assignee avatar */}
      <div className="w-8 shrink-0">
        {assignee ? (
          <Avatar
            name={assignee.member.display_name}
            src={assignee.member.avatar_url || undefined}
            size="sm"
            showTooltip={true}
          />
        ) : (
          <div className="flex items-center justify-center">
            <div className="grid size-6 place-items-center rounded-full bg-custom-background-80 text-[10px] text-custom-text-400">
              —
            </div>
          </div>
        )}
      </div>

      {/* State badge */}
      <div className="w-24 shrink-0">
        {state ? (
          <span
            className="inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs"
            style={{
              backgroundColor: `${state.color}1A`,
              color: state.color,
            }}
          >
            <span className="size-1.5 rounded-full" style={{ backgroundColor: state.color }} />
            {state.name}
          </span>
        ) : (
          <span className="text-xs text-custom-text-400">—</span>
        )}
      </div>

      {/* Updated time */}
      <span className="w-20 shrink-0 text-right text-xs text-custom-text-400">
        {formatDistanceToNow(new Date(issue.updated_at), {
          addSuffix: true,
          locale: zhCN,
        })}
      </span>
    </div>
  );
};
