// FLOW: Issue detail route page — dual-column layout per D-P16-09
"use client";

import { observer } from "mobx-react";
import { useParams, useNavigate } from "react-router";
import { ArrowLeft } from "lucide-react";
import type { TIssue } from "@plane/types";
import { useIssue, useIssueMutations } from "@/../src/lib/hooks/use-issues";
import { IssueDetailMain } from "@/components/issues/issue-detail-main";
import { IssueDetailSidebar } from "@/components/issues/issue-detail-sidebar";
import { CommentList } from "@/components/issues/comment-list";
import { CommentInput } from "@/components/issues/comment-input";
import { ActivityLog } from "@/components/issues/activity-log";
import { Loader } from "@plane/ui";

const IssueDetailPage = observer(function IssueDetailPage() {
  const { workspaceId, projectId, issueId } = useParams<{
    workspaceId: string;
    projectId: string;
    issueId: string;
  }>();
  const navigate = useNavigate();
  const { data: issue, isLoading } = useIssue(projectId ?? "", issueId ?? "");
  const { updateIssue } = useIssueMutations();

  const handleUpdate = (data: Partial<TIssue>) => {
    if (!issueId) return;
    updateIssue.mutate({ issueId, data });
  };

  // Loading state
  if (isLoading) {
    return (
      <div className="flex h-full flex-col gap-4 p-6">
        <Loader className="flex gap-4">
          <div className="flex-[2] space-y-4">
            <Loader.Item height="40px" width="60%" />
            <Loader.Item height="200px" />
          </div>
          <div className="flex-[1] space-y-4">
            <Loader.Item height="30px" />
            <Loader.Item height="30px" />
            <Loader.Item height="30px" />
          </div>
        </Loader>
      </div>
    );
  }

  // Not found state
  if (!issue) {
    return (
      <div className="flex h-full items-center justify-center">
        <p className="text-sm text-custom-text-400">Issue 不存在或已被删除</p>
      </div>
    );
  }

  return (
    <div className="flex h-full flex-col">
      {/* Top navigation bar */}
      <div className="border-custom-border-200 flex items-center gap-2 border-b px-6 py-3">
        <button
          onClick={() => navigate(-1)}
          className="text-sm text-custom-text-300 hover:text-custom-text-100 flex items-center gap-1.5 transition-colors"
          aria-label="返回"
        >
          <ArrowLeft className="size-4" />
          <span>Back to Issues</span>
        </button>
      </div>

      {/* Dual-column layout per D-P16-09 */}
      <div className="flex flex-1 gap-4 overflow-y-auto p-6">
        {/* Left column ~65% */}
        <div className="flex flex-[2] flex-col gap-6">
          <IssueDetailMain issue={issue} workspaceId={workspaceId ?? ""} onUpdate={handleUpdate} />

          {/* Comments section per D-P16-11 */}
          <section>
            <CommentList issueId={issueId ?? ""} workspaceId={workspaceId ?? ""} projectId={projectId ?? ""} />
            <div className="mt-4">
              <CommentInput issueId={issueId ?? ""} />
            </div>
          </section>

          {/* Activity Log section */}
          <hr className="border-custom-border-200" />
          <ActivityLog issueId={issueId ?? ""} />
        </div>

        {/* Right column ~35% */}
        <div className="flex-[1]">
          <IssueDetailSidebar
            issue={issue}
            workspaceId={workspaceId ?? ""}
            projectId={projectId ?? ""}
            onUpdate={handleUpdate}
          />
        </div>
      </div>
    </div>
  );
});

export default IssueDetailPage;
