// FLOW: Views 列表页入口路由组件（per D-P19-01）
"use client";
import { observer } from "mobx-react";
import { useParams } from "react-router";
import { ViewsList } from "@/components/views/views-list";

const ViewsPage = observer(function ViewsPage() {
  const { workspaceId, projectId } = useParams<{ workspaceId: string; projectId: string }>();

  if (!workspaceId || !projectId) {
    return (
      <div className="flex h-full items-center justify-center">
        <p className="text-sm text-custom-text-300">参数错误</p>
      </div>
    );
  }

  return <ViewsList workspaceId={workspaceId} projectId={projectId} />;
});

export default ViewsPage;
