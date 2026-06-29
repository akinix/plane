// FLOW: ActivityList — recent activity list for workspace dashboard (D-P15-09)
import type { TProjectBaseActivity } from "@plane/types";

type TActivityListProps = {
  activities: TProjectBaseActivity[];
};

// Simple relative time formatter
const timeAgo = (epoch: number): string => {
  const now = Date.now();
  const diff = now - epoch * 1000;
  const mins = Math.floor(diff / 60000);
  if (mins < 1) return "刚刚";
  if (mins < 60) return `${mins} 分钟前`;
  const hours = Math.floor(mins / 60);
  if (hours < 24) return `${hours} 小时前`;
  const days = Math.floor(hours / 24);
  if (days < 30) return `${days} 天前`;
  return `${Math.floor(days / 30)} 个月前`;
};

const verbLabel = (verb: string, field?: string): string => {
  if (verb === "created") return `创建了${field ? ` ${field}` : ""}`;
  if (verb === "updated") return `更新了${field ? ` ${field}` : ""}`;
  if (verb === "deleted") return `删除了${field ? ` ${field}` : ""}`;
  return `${verb}${field ? ` ${field}` : ""}`;
};

export const ActivityList = ({ activities }: TActivityListProps) => {
  if (!activities || activities.length === 0) {
    return (
      <p className="py-4 text-center text-sm text-custom-text-300">
        暂无活动记录
      </p>
    );
  }

  return (
    <div className="flex flex-col">
      {activities.map((activity) => (
        <div
          key={activity.id}
          className="flex items-start gap-3 border-b border-custom-border-200 py-3 last:border-b-0"
        >
          {/* Actor avatar */}
          <div className="flex size-7 flex-shrink-0 items-center justify-center rounded-full bg-custom-background-80 text-xs font-medium text-custom-text-300">
            {activity.actor.charAt(0).toUpperCase()}
          </div>

          {/* Activity content */}
          <div className="min-w-0 flex-1">
            <p className="text-xs text-custom-text-200">
              <span className="font-medium text-custom-text-100">
                {activity.actor}
              </span>{" "}
              {verbLabel(activity.verb, activity.field)}
              {activity.comment && (
                <span className="text-custom-text-300">
                  ：{activity.comment}
                </span>
              )}
            </p>
          </div>

          {/* Timestamp */}
          <span className="flex-shrink-0 text-xs text-custom-text-400">
            {timeAgo(activity.epoch)}
          </span>
        </div>
      ))}
    </div>
  );
};
