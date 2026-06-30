// FLOW: Forked from Plane. Original: apps/web/core/components/pages/header/archived-badge.tsx
// FLOW: PageArchivedBadge — 已归档页面徽章
"use client";
import { Archive } from "lucide-react";
import type { TPage } from "@plane/types";
import { renderFormattedDate } from "@plane/utils";

type Props = {
  page: TPage;
};

export const PageArchivedBadge = function PageArchivedBadge({ page }: Props) {
  const { archived_at } = page;

  if (!archived_at) return null;

  return (
    <div className="bg-custom-primary/20 text-custom-primary flex h-6 flex-shrink-0 items-center gap-1 rounded-sm px-2">
      <Archive className="size-3.5 flex-shrink-0" />
      <span className="text-[11px] font-medium">已归档于 {renderFormattedDate(archived_at)}</span>
    </div>
  );
};
