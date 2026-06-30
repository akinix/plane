// FLOW: Forked from Plane. Original: apps/web/core/components/pages/editor/page-root.tsx
// FLOW: PageEditorRoot — 编辑器全页容器（Header + Title + EditorBody + 自动保存 + NavigationPane）
"use client";
import { useCallback, useEffect, useRef, useState } from "react";
import { observer } from "mobx-react";
import { usePageDetail } from "@/../src/lib/hooks/use-pages";
import { usePageMutations } from "@/../src/lib/hooks/use-page-mutations";
import { PageLoader } from "@/components/pages/loaders/page-loader";
import { PageEditorBody } from "./editor-body";
import { PageEditorTitle } from "./title";
import { PageEditorHeaderRoot } from "./header/root";
import { PageEditorToolbarRoot } from "./toolbar/root";
import { PageNavigationPaneRoot } from "@/components/pages/navigation-pane/root";

type Props = {
  workspaceId: string;
  pageId: string;
};

export const PageEditorRoot = observer(function PageEditorRoot({ workspaceId, pageId }: Props) {
  const { data: page, isLoading, isError } = usePageDetail(workspaceId, pageId);
  const { updatePage } = usePageMutations();
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const [isNavigationPaneOpen, setIsNavigationPaneOpen] = useState(false);

  // Auto-save logic: debounce 1500ms on editor content change
  const handleEditorChange = useCallback(
    (_json: object, html: string) => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
      debounceRef.current = setTimeout(() => {
        updatePage.mutate({ pageId, data: { description_html: html } });
      }, 1500);
    },
    [pageId, updatePage]
  );

  // Cleanup debounce on unmount
  useEffect(() => {
    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
    };
  }, []);

  // Handle error state
  if (isError) {
    return (
      <div className="flex size-full items-center justify-center">
        <div className="text-center">
          <p className="text-custom-text-200 text-sm">页面加载失败</p>
          <button
            type="button"
            onClick={() => window.location.reload()}
            className="text-custom-primary text-sm mt-2 underline"
          >
            重试
          </button>
        </div>
      </div>
    );
  }

  // Handle loading state
  if (isLoading || !page) {
    return <PageLoader />;
  }

  return (
    <div className="relative flex size-full overflow-hidden transition-all duration-300 ease-in-out">
      <div className="flex size-full flex-col overflow-hidden">
        <PageEditorToolbarRoot
          isNavigationPaneOpen={isNavigationPaneOpen}
          onOpenNavigationPane={() => setIsNavigationPaneOpen(true)}
        />
        <div className="vertical-scrollbar relative flex size-full flex-col overflow-x-hidden overflow-y-auto">
          <div className="mx-auto block w-full max-w-[720px] bg-transparent">
            <div className="page-header-container group/page-header">
              <PageEditorHeaderRoot
                page={page}
                workspaceId={workspaceId}
                onToggleNavigationPane={() => setIsNavigationPaneOpen((prev) => !prev)}
              />
            </div>
            <PageEditorTitle pageId={pageId} title={page.name} workspaceId={workspaceId} />
          </div>
          <PageEditorBody
            onChange={handleEditorChange}
            initialValue={page.description_html ?? ""}
            editable={!page.is_locked}
            id={pageId}
          />
        </div>
      </div>
      <PageNavigationPaneRoot
        isOpen={isNavigationPaneOpen}
        onClose={() => setIsNavigationPaneOpen(false)}
        page={page}
      />
    </div>
  );
});
