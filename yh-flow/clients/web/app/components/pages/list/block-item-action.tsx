// FLOW: Forked from Plane. Original: apps/web/core/components/pages/list/block-item-action.tsx
// FLOW: BlockItemAction — 页面卡片悬停操作按钮
"use client";
import { Earth, Lock, Info } from "lucide-react";
import type { TPage } from "@plane/types";
import { EPageAccess } from "@plane/types";
import { PageActions } from "../dropdowns/actions";
import { renderFormattedDate } from "@plane/utils";
import { Tooltip } from "@plane/ui";

type Props = {
  page: TPage;
  workspaceId: string;
  parentRef?: React.RefObject<HTMLElement>;
};

export const BlockItemAction = function BlockItemAction({ page, workspaceId, parentRef }: Props) {
  const { id, access, created_at } = page;

  if (!id) return null;

  return (
    <>
      {/* Access icon */}
      <div className="cursor-default">
        <Tooltip tooltipContent={access === EPageAccess.PUBLIC ? "公开" : "私人"}>
          {access === EPageAccess.PUBLIC ? (
            <Earth className="text-custom-text-300 size-4" />
          ) : (
            <Lock className="text-custom-text-300 size-4" />
          )}
        </Tooltip>
      </div>

      {/* Created date */}
      <Tooltip tooltipContent={`创建于 ${created_at ? renderFormattedDate(created_at) : ""}`}>
        <span className="grid size-4 cursor-default place-items-center">
          <Info className="text-custom-text-300 size-4" />
        </span>
      </Tooltip>

      {/* Quick actions dropdown */}
      <PageActions page={page} workspaceId={workspaceId} parentRef={parentRef} />
    </>
  );
};
