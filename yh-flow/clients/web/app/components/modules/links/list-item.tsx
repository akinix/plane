// FLOW: Forked from Plane modules/links/list-item.tsx
// FLOW: ModulesLinksListItem — Module 链接列表项（链接标题、URL、删除按钮、URL 校验）
import React from "react";
import { Edit3, Trash2, Copy, ExternalLink } from "lucide-react";
import type { ILinkDetails } from "@plane/types";
import { copyTextToClipboard } from "@plane/utils";

type Props = {
  link: ILinkDetails;
  onEdit: () => void;
  onDelete: () => void;
  isEditingAllowed?: boolean;
};

/**
 * 基础 URL 校验：必须 http:// 或 https:// 开头
 */
function isValidUrl(url: string): boolean {
  return /^https?:\/\/.+/.test(url);
}

export const ModulesLinksListItem = React.memo(function ModulesLinksListItem(props: Props) {
  const { link, onEdit, onDelete, isEditingAllowed = true } = props;

  const handleCopy = () => {
    copyTextToClipboard(link.url);
  };

  return (
    <div className="relative flex flex-col rounded-md bg-layer-3 p-2.5">
      <div className="flex w-full items-start justify-between gap-2">
        <div className="flex items-start gap-2 truncate">
          <span className="py-1">
            <ExternalLink className="size-3 shrink-0 stroke-2 text-tertiary" />
          </span>
          <a
            href={link.url}
            target="_blank"
            rel="noopener noreferrer"
            className="cursor-pointer truncate text-11 hover:underline"
            title={link.title || link.url}
          >
            {link.title || link.url}
          </a>
        </div>

        <div className="z-1 flex shrink-0 items-center">
          <button
            type="button"
            onClick={handleCopy}
            className="grid place-items-center rounded-sm p-1 text-secondary hover:bg-layer-transparent-hover"
            title="复制链接"
          >
            <Copy className="size-3 stroke-[1.5]" />
          </button>
          {isEditingAllowed && (
            <>
              <button
                type="button"
                onClick={onEdit}
                className="grid place-items-center rounded-sm p-1 text-secondary hover:bg-layer-transparent-hover"
                title="编辑链接"
              >
                <Edit3 className="size-3 stroke-[1.5]" />
              </button>
              <button
                type="button"
                onClick={onDelete}
                className="grid place-items-center rounded-sm p-1 text-secondary hover:bg-layer-transparent-hover"
                title="删除链接"
              >
                <Trash2 className="size-3 stroke-[1.5]" />
              </button>
            </>
          )}
        </div>
      </div>
      <div className="px-5">
        <p className="mt-0.5 flex items-center gap-1.5 text-11 text-tertiary">
          添加于 {new Date(link.created_at).toLocaleDateString("zh-CN")}
        </p>
      </div>
    </div>
  );
});

export { isValidUrl };
