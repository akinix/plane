// FLOW: ActivityLog — issue activity log showing property change history
"use client";

import { observer } from "mobx-react";
import { formatDistanceToNow } from "date-fns";
import { zhCN } from "date-fns/locale";
import { Avatar } from "@plane/ui";
import { MOCK_ISSUE_ACTIVITIES } from "@/../src/lib/mock-data";

type Props = {
  issueId: string;
};

/**
 * Generates a Chinese description for an activity entry based on field, verb, old_value, new_value.
 */
function describeActivity(
  field: string | undefined,
  verb: string,
  oldValue: string | undefined,
  newValue: string | undefined,
  comment: string | undefined
): string {
  if (comment) return comment;

  const fieldLabels: Record<string, string> = {
    name: "名称",
    state: "状态",
    priority: "优先级",
    assignee: "负责人",
    label: "标签",
    description: "描述",
    estimate_point: "估算",
    target_date: "截止日期",
    start_date: "开始日期",
  };

  const fieldLabel = fieldLabels[field ?? ""] ?? field;

  if (verb === "created") {
    return field === "name" || !field ? "创建了 Issue" : `设置了 ${fieldLabel}`;
  }

  if (verb === "updated" || verb === "changed") {
    if (field === "description") return "更新了 Issue 描述";

    if (newValue && oldValue) {
      return `将 ${fieldLabel} 从「${oldValue}」改为「${newValue}」`;
    }
    if (newValue && !oldValue) {
      return `将 ${fieldLabel} 设置为「${newValue}」`;
    }
    return `更新了 ${fieldLabel}`;
  }

  return `更新了 ${fieldLabel}`;
}

export const ActivityLog = observer(function ActivityLog({ issueId }: Props) {
  const activities = MOCK_ISSUE_ACTIVITIES.filter((a) => a.issue === issueId);
  const sorted = [...activities].toSorted(
    (a, b) => new Date(b.created_at).getTime() - new Date(a.created_at).getTime()
  );

  if (sorted.length === 0) {
    return (
      <div>
        <h4 className="text-custom-text-100 mb-3 text-h4-semibold">活动日志</h4>
        <p className="text-sm text-custom-text-400">暂无活动记录</p>
      </div>
    );
  }

  return (
    <div>
      <h4 className="text-custom-text-100 mb-3 text-h4-semibold">活动日志</h4>
      <div className="flex flex-col">
        {sorted.map((activity, index) => (
          <div key={activity.id}>
            <div className="flex gap-3 py-3">
              {/* Avatar */}
              <div className="mt-0.5 flex-shrink-0">
                <Avatar name={activity.actor_detail.display_name} src={activity.actor_detail.avatar_url} size="sm" />
              </div>

              {/* Activity content */}
              <div className="min-w-0 flex-1">
                <div className="flex items-center gap-2">
                  <span className="text-sm text-custom-text-100 font-medium">{activity.actor_detail.display_name}</span>
                  <span className="text-xs text-custom-text-400">
                    {formatDistanceToNow(new Date(activity.created_at), {
                      addSuffix: true,
                      locale: zhCN,
                    })}
                  </span>
                </div>
                <p className="text-sm text-custom-text-300 mt-0.5">
                  {describeActivity(
                    activity.field,
                    activity.verb,
                    activity.old_value,
                    activity.new_value,
                    activity.comment
                  )}
                </p>
              </div>
            </div>

            {/* Separator */}
            {index < sorted.length - 1 && <hr className="border-custom-border-200" />}
          </div>
        ))}
      </div>
    </div>
  );
});
