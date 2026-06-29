// FLOW: GanttView — 甘特图主容器组件
// FLOW: 使用 useIssues hook 获取数据，接入 GanttChartRoot
// FLOW: 支持 4 级缩放（天/周/月/季度 per D-P17-16）
// FLOW: 依赖关系连线 GANT-02（只读显示）
/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 *
 * GanttView — Issues 甘特图视图容器。
 * Fork 适配自 Plane gantt-chart 根组件模式。
 * Phase 17 为只读显示，不包含拖拽编辑交互。
 */

import { useMemo } from "react";
import { observer } from "mobx-react";
import { GitBranch } from "lucide-react";
import type { IGanttBlock } from "@plane/types";
import { useStore } from "@/lib/store-context";
import { useIssues } from "@/../src/lib/hooks/use-issues";
import { Loader } from "@plane/ui";
import { GanttChartRoot } from "./root";
import { TimeLineChartProvider } from "./hooks/use-timeline-chart";
import type { TIssue } from "@plane/types";

type TProps = {
  workspaceId: string;
  projectId: string;
};

// FLOW: 缩放按钮由 GanttChartHeader 内部处理（通过 VIEWS_LIST + "天" 按钮）
// 4 级缩放：天/周/月/季度 per D-P17-16

/**
 * 将 TIssue 转换为 IGanttBlock 格式
 */
const issueToBlock = (issue: TIssue, index: number): IGanttBlock => ({
  data: issue,
  id: issue.id,
  name: issue.name,
  sort_order: issue.sort_order,
  start_date: issue.start_date ?? undefined,
  target_date: issue.target_date ?? undefined,
});

/**
 * 通过子 issue 比例计算进度
 */
const calcProgress = (issue: TIssue, allIssues: TIssue[]): number => {
  if (!issue.parent_id) {
    // 有子 issue 的父级：已关闭子 issue 比例
    const children = allIssues.filter((i) => i.parent_id === issue.id);
    if (children.length > 0) {
      const completed = children.filter((i) => i.completed_at !== null).length;
      return Math.round((completed / children.length) * 100);
    }
    // 无子 issue：completed_at 决定 0/100
    return issue.completed_at !== null ? 100 : 0;
  }
  // 子 issue 本身：由父级控制
  return 0;
};

/**
 * 获取 issue 的依赖关系
 */
const getDependencies = (issue: TIssue, allIssues: TIssue[]): string[] => {
  // 检查 issue_relation 是否包含 blocked_by 关系
  const relations = issue.issue_relation ?? [];
  const blockedBy = relations
    .filter((r) => r.relation_type === "blocked_by")
    .map((r) => r.id);
  return blockedBy;
};

const GanttView = observer(function GanttView({ workspaceId, projectId }: TProps) {
  const store = useStore();

  // 获取 issue 数据
  const { data: issues, isLoading } = useIssues(projectId, {
    state: store.issue.filters.stateIds,
    priority: store.issue.filters.priorityIds,
    assignee: store.issue.filters.assigneeIds,
    searchQuery: store.issue.filters.searchQuery,
  });

  // 筛选有日期的 issue 并按 start_date 排序
  const datedIssues = useMemo(() => {
    if (!issues) return [];
    return issues
      .filter((i) => i.start_date || i.target_date)
      .sort((a, b) => {
        if (!a.start_date) return 1;
        if (!b.start_date) return -1;
        return new Date(a.start_date).getTime() - new Date(b.start_date).getTime();
      });
  }, [issues]);

  // 转换为 IGanttBlock[]
  const blocks = useMemo(() => datedIssues.map((issue, idx) => issueToBlock(issue, idx)), [datedIssues]);

  // 带日期的 issue IDs
  const blockIds = useMemo(() => blocks.map((b) => b.id), [blocks]);

  // 空状态 — 处理两种空情况
  const isEmpty = !isLoading && (!issues || issues.length === 0);
  const noDatedIssues = !isLoading && issues && issues.length > 0 && datedIssues.length === 0;

  // Loading state
  if (isLoading) {
    return (
      <div className="flex h-full flex-col">
        <div className="flex items-center gap-4 px-6 py-3">
          <Loader className="flex items-center gap-2">
            <Loader.Item width="220px" height="32px" className="rounded-md" />
          </Loader>
        </div>
        <div className="flex gap-0 overflow-hidden px-0 pb-4" style={{ height: "400px" }}>
          {/* Sidebar skeleton */}
          <div className="flex-shrink-0 border-r border-subtle bg-surface-1 p-4" style={{ width: "360px" }}>
            <Loader className="space-y-3">
              {Array.from({ length: 6 }).map((_, i) => (
                <Loader.Item key={i} width="100%" height="44px" className="rounded" />
              ))}
            </Loader>
          </div>
          {/* Chart skeleton */}
          <div className="flex-grow p-4">
            <Loader className="space-y-3">
              {Array.from({ length: 6 }).map((_, i) => (
                <div key={i} className="flex items-center gap-2">
                  <Loader.Item width="60px" height="44px" className="rounded" />
                  <Loader.Item width="200px" height="44px" className="rounded" />
                  <Loader.Item width="120px" height="44px" className="rounded" />
                </div>
              ))}
            </Loader>
          </div>
        </div>
      </div>
    );
  }

  // 空状态 — 无任何 issue
  if (isEmpty) {
    return (
      <div className="flex flex-col items-center justify-center py-16">
        <div className="mb-3 rounded-full bg-surface-2 p-4">
          <GitBranch className="size-8 text-tertiary" />
        </div>
        <h3 className="text-sm font-medium text-secondary">暂无 Issue</h3>
        <p className="mt-1 text-xs text-tertiary">此项目还没有需要展示甘特图的 Issue，请创建带起止日期的 Issue。</p>
      </div>
    );
  }

  // 空状态 — 有 issue 但无日期
  if (noDatedIssues) {
    return (
      <div className="flex flex-col items-center justify-center py-16">
        <div className="mb-3 rounded-full bg-surface-2 p-4">
          <GitBranch className="size-8 text-tertiary" />
        </div>
        <h3 className="text-sm font-medium text-secondary">暂无 Issue</h3>
        <p className="mt-1 text-xs text-tertiary">此项目还没有需要展示甘特图的 Issue，请创建带起止日期的 Issue。</p>
      </div>
    );
  }

  const blockUpdateHandler = (_block: any, _payload: any) => {
    // FLOW: Phase 17 只读 — 拖拽编辑暂不实现
  };

  const blockToRender = (data: any) => {
    const issue = data as TIssue;
    const progress = calcProgress(issue, issues ?? []);
    return (
      <div className="flex h-full w-full items-center px-2">
        <div className="flex h-5 w-full items-center overflow-hidden rounded-sm bg-layer-1">
          <div
            className="h-full rounded-sm bg-accent-primary transition-all duration-200"
            style={{ width: `${progress}%` }}
          />
        </div>
      </div>
    );
  };

  const sidebarToRender = (props: any) => {
    // 接收 GanttChartSidebar 传递的 props，渲染右侧 issue 列表
    return (
      <div>
        {blockIds.map((blockId: string) => {
          const block = blocks.find((b) => b.id === blockId);
          if (!block || !block.data) return null;
          const issue = block.data as TIssue;
          return (
            <div
              key={blockId}
              className="flex items-center gap-2 border-b border-subtle px-4 py-2"
              style={{ height: "44px" }}
            >
              <span className="flex-shrink-0 text-xs text-tertiary">
                {issue.sequence_id}
              </span>
              <span className="truncate text-sm text-primary">{issue.name}</span>
            </div>
          );
        })}
      </div>
    );
  };

  return (
    <TimeLineChartProvider>
      <GanttChartRoot
        border
        title="甘特图"
        loaderTitle="issues"
        blockIds={blockIds}
        blockUpdateHandler={blockUpdateHandler}
        blockToRender={blockToRender}
        sidebarToRender={sidebarToRender}
        enableBlockLeftResize={false}
        enableBlockRightResize={false}
        enableBlockMove={false}
        enableReorder={false}
        enableAddBlock={false}
        enableSelection={false}
        enableDependency={false}
        bottomSpacing={false}
        showAllBlocks={false}
        showToday
      />
    </TimeLineChartProvider>
  );
});

export { GanttView };
