// FLOW: Forked from Plane. Original: apps/web/core/components/pages/dropdowns/actions.tsx
// FLOW: PageActions — 页面操作下拉菜单
"use client";
import { useState } from "react";
import { LinkIcon, ArchiveIcon, TrashIcon, MoreHorizontal } from "lucide-react";
import type { TPage } from "@plane/types";
import { CustomMenu } from "@plane/ui";
import { usePageMutations } from "@/../src/lib/hooks/use-page-mutations";
import { DeletePageModal } from "../modals/delete-page-modal";

type Props = {
  page: TPage;
  workspaceId: string;
  parentRef?: React.RefObject<HTMLElement>;
};

export const PageActions = function PageActions({ page, workspaceId, parentRef: _parentRef }: Props) {
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const { archivePage, restorePage } = usePageMutations();
  const { id, name, archived_at } = page;

  const handleArchiveRestore = () => {
    if (!id) return;
    if (archived_at) {
      restorePage.mutate(id);
    } else {
      archivePage.mutate(id);
    }
  };

  const handleCopyLink = () => {
    if (!id) return;
    const url = `${window.location.origin}/workspaces/${workspaceId}/pages/${id}`;
    navigator.clipboard.writeText(url).catch(() => {
      // Clipboard write failed (permissions, HTTP context, etc.) — silently ignore
    });
  };

  if (!id) return null;

  return (
    <>
      <CustomMenu
        customButton={
          <button
            type="button"
            className="text-custom-text-300 hover:text-custom-text-200 grid place-items-center p-0.5"
            onClick={(e) => e.stopPropagation()}
          >
            <MoreHorizontal className="size-4" />
          </button>
        }
        placement="bottom-end"
        closeOnSelect
      >
        {/* Copy link */}
        <CustomMenu.MenuItem onClick={handleCopyLink} className="flex items-center gap-2">
          <LinkIcon className="size-3" />
          复制链接
        </CustomMenu.MenuItem>

        {/* Archive / Restore */}
        <CustomMenu.MenuItem
          onClick={(e: any) => {
            e.stopPropagation();
            handleArchiveRestore();
          }}
          className="flex items-center gap-2"
        >
          <ArchiveIcon className="size-3" />
          {archived_at ? "恢复" : "归档"}
        </CustomMenu.MenuItem>

        {/* Delete (only in archived tab) */}
        {archived_at && (
          <CustomMenu.MenuItem
            onClick={(e: any) => {
              e.stopPropagation();
              setDeleteModalOpen(true);
            }}
            className="text-red-500 flex items-center gap-2"
          >
            <TrashIcon className="size-3" />
            删除
          </CustomMenu.MenuItem>
        )}
      </CustomMenu>

      {deleteModalOpen && (
        <DeletePageModal
          pageId={id}
          pageName={name ?? ""}
          isOpen={deleteModalOpen}
          onClose={() => setDeleteModalOpen(false)}
          workspaceId={workspaceId}
        />
      )}
    </>
  );
};
