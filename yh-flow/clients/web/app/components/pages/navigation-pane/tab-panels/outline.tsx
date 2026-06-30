// FLOW: Forked from Plane. Original: apps/web/core/components/pages/navigation-pane/tab-panels/outline.tsx
// FLOW: PageNavigationPaneOutlineTabPanel — 文档大纲（解析 heading 标签）
"use client";
import { useMemo } from "react";
import type { TPage } from "@plane/types";
import { cn } from "@plane/utils";

type Props = {
  page: TPage;
};

type Heading = {
  level: number;
  text: string;
  id: string;
};

export const PageNavigationPaneOutlineTabPanel = function PageNavigationPaneOutlineTabPanel({ page }: Props) {
  const headings = useMemo<Heading[]>(() => {
    if (!page.description_html) return [];
    const regex = /<h([1-6])[^>]*>(.*?)<\/h\1>/gi;
    const results: Heading[] = [];
    let match: RegExpExecArray | null;
    let index = 0;
    while ((match = regex.exec(page.description_html)) !== null) {
      const level = parseInt(match[1], 10);
      const text = match[2].replace(/<[^>]*>/g, "").trim();
      if (text) {
        results.push({ level, text, id: `heading-${index++}` });
      }
    }
    return results;
  }, [page.description_html]);

  if (headings.length === 0) {
    return <div className="text-xs text-custom-text-400 py-4 text-center">暂无标题内容</div>;
  }

  return (
    <div className="space-y-1">
      {headings.map((heading) => (
        <button
          type="button"
          key={heading.id}
          className={cn(
            "text-xs text-custom-text-400 hover:text-custom-text-100 w-full cursor-pointer py-1 text-left transition-colors",
            heading.level === 1 && "pl-0 font-medium",
            heading.level === 2 && "pl-3",
            heading.level === 3 && "pl-6",
            heading.level >= 4 && "pl-9"
          )}
        >
          {heading.text}
        </button>
      ))}
    </div>
  );
};
