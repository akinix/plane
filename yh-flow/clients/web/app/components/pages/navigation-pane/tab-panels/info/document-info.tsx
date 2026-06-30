// FLOW: Forked from Plane. Original: apps/web/core/components/pages/navigation-pane/tab-panels/info/document-info.tsx
// FLOW: PageNavigationPaneInfoTabDocumentInfo — 文档元信息
"use client";
import { useMemo } from "react";
import type { TPage } from "@plane/types";
import { renderFormattedDate } from "@plane/utils";

type Props = {
  page: TPage;
};

export const PageNavigationPaneInfoTabDocumentInfo = function PageNavigationPaneInfoTabDocumentInfo({ page }: Props) {
  const infoItems = useMemo(
    () => [
      { label: "创建人", value: page.created_by || "-" },
      { label: "创建时间", value: page.created_at ? renderFormattedDate(page.created_at) : "-" },
      { label: "更新人", value: page.updated_by || "-" },
      { label: "更新时间", value: page.updated_at ? renderFormattedDate(page.updated_at) : "-" },
    ],
    [page]
  );

  return (
    <div className="space-y-3">
      <h4 className="text-xs text-custom-text-400 font-semibold">文档信息</h4>
      <div className="space-y-2">
        {infoItems.map((item) => (
          <div key={item.label} className="flex flex-col">
            <span className="text-xs text-custom-text-400">{item.label}</span>
            <span className="text-sm text-custom-text-200">{item.value}</span>
          </div>
        ))}
      </div>
    </div>
  );
};
