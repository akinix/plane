// FLOW: Forked from Plane. Original: apps/web/app/workspace-slug/projects/project-id/pages/page-id/page.tsx
// FLOW: PageEditorPage — 编辑器页面路由入口
"use client";
import { observer } from "mobx-react";
import { useParams } from "react-router";
import { PageEditorRoot } from "@/components/pages/editor/page-root";

const PageEditorPage = observer(function PageEditorPage() {
  const { workspaceId, pageId } = useParams<{ workspaceId: string; pageId: string }>();
  if (!workspaceId || !pageId) return null;
  return <PageEditorRoot workspaceId={workspaceId} pageId={pageId} />;
});
export default PageEditorPage;
