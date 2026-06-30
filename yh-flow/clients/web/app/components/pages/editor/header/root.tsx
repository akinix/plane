// FLOW: Forked from Plane. Original: apps/web/core/components/pages/editor/header/root.tsx
// FLOW: PageEditorHeaderRoot — 编辑器 Header 容器（返回按钮 + 访问权限切换 + 收藏 + 归档/删除）
"use client";
import { useState } from "react";
import { useNavigate } from "react-router";
import { ArrowLeft, Globe, Lock, MoreHorizontal } from "lucide-react";
import type { TPage } from "@plane/types";
import { EPageAccess } from "@plane/types";
import { CustomMenu } from "@plane/ui";
import { cn } from "@plane/utils";
import { usePageMutations } from "@/../src/lib/hooks/use-page-mutations";
import { useStore } from "@/lib/store-context";
import { PageFavoriteControl } from "@/components/pages/header/favorite-control";
import { DeletePageModal } from "@/components/pages/modals/delete-page-modal";

type Props = {
  page: TPage;
  workspaceId: string;
  onToggleNavigationPane: () => void;
};

export const PageEditorHeaderRoot = function PageEditorHeaderRoot({
  page,
  workspaceId,
  onToggleNavigationPane: _onToggleNavigationPane,
}: Props) {
  const navigate = useNavigate();
  const { updatePage } = usePageMutations();
  const store = useStore();
  const [accessMenuOpen, setAccessMenuOpen] = useState(false);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);

  const accessLabel = page.access === EPageAccess.PUBLIC ? "公开" : "私人";
  const AccessIcon = page.access === EPageAccess.PUBLIC ? Globe : Lock;

  const handleAccessChange = (access: EPageAccess) => {
    updatePage.mutate({ pageId: page.id, data: { access } });
    setAccessMenuOpen(false);
  };

  const handleArchive = () => {
    store.page.openArchiveModal(page.id, page.name);
  };

  const handleDelete = () => {
    setDeleteModalOpen(true);
  };

  return (
    <>
      <div className="flex h-[48px] items-center gap-2">
        <button
          type="button"
          onClick={() => navigate(`/workspaces/${workspaceId}/pages`)}
          className="text-custom-text-400 hover:bg-custom-background-80 grid size-7 place-items-center rounded transition-colors"
          aria-label="返回页面列表"
        >
          <ArrowLeft className="size-4" />
        </button>

        {/* Page name (read-only in header) */}
        <span className="text-sm text-custom-text-200 max-w-[200px] truncate font-medium">{page.name || "无标题"}</span>

        <div className="ml-auto flex items-center gap-1">
          {/* Access toggle */}
          <CustomMenu
            customButton={
              <button
                type="button"
                className={cn(
                  "text-xs flex items-center gap-1.5 rounded px-2 py-1 transition-colors",
                  "text-custom-text-300 hover:bg-custom-background-80",
                  { "bg-custom-background-80": accessMenuOpen }
                )}
                onClick={() => setAccessMenuOpen(!accessMenuOpen)}
              >
                <AccessIcon className="size-3.5" />
                {accessLabel}
              </button>
            }
            placement="bottom-end"
          >
            <CustomMenu.MenuItem onClick={() => handleAccessChange(EPageAccess.PUBLIC)}>
              <Globe className="size-3.5" />
              公开
            </CustomMenu.MenuItem>
            <CustomMenu.MenuItem onClick={() => handleAccessChange(EPageAccess.PRIVATE)}>
              <Lock className="size-3.5" />
              私人
            </CustomMenu.MenuItem>
          </CustomMenu>

          {/* Favorite */}
          <PageFavoriteControl page={page} />

          {/* More actions */}
          <CustomMenu
            customButton={
              <button
                type="button"
                className="text-custom-text-400 hover:bg-custom-background-80 grid size-7 place-items-center rounded transition-colors"
                aria-label="更多操作"
              >
                <MoreHorizontal className="size-4" />
              </button>
            }
            placement="bottom-end"
          >
            <CustomMenu.MenuItem onClick={handleArchive}>归档</CustomMenu.MenuItem>
            <CustomMenu.MenuItem onClick={handleDelete}>删除</CustomMenu.MenuItem>
          </CustomMenu>
        </div>
      </div>

      {/* Delete modal */}
      <DeletePageModal
        pageId={page.id}
        pageName={page.name}
        isOpen={deleteModalOpen}
        onClose={() => setDeleteModalOpen(false)}
        workspaceId={workspaceId}
      />
    </>
  );
};
