// FLOW: Forked from Plane pages-list-main-content.tsx usage
"use client";
import { observer } from "mobx-react";
import { useParams } from "react-router";
import { PagesListMainContent } from "@/components/pages/pages-list-main-content";

const PagesPage = observer(function PagesPage() {
  const { workspaceId } = useParams<{ workspaceId: string }>();
  if (!workspaceId) return null;
  return (
    <div className="flex h-full flex-col">
      <PagesListMainContent workspaceId={workspaceId} />
    </div>
  );
});
export default PagesPage;
