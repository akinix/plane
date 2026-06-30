// FLOW: Forked from Plane. Original: apps/web/core/components/pages/editor/title.tsx
// FLOW: PageEditorTitle — 页面标题编辑输入框
"use client";
import { useRef, useState, useCallback, useEffect } from "react";
import { usePageMutations } from "@/../src/lib/hooks/use-page-mutations";

type Props = {
  pageId: string;
  title: string | undefined;
  workspaceId: string;
};

export const PageEditorTitle = function PageEditorTitle({ pageId, title, workspaceId: _workspaceId }: Props) {
  const [isEditing, setIsEditing] = useState(false);
  const [localTitle, setLocalTitle] = useState(title ?? "");

  // Sync prop into local state when title loads asynchronously
  useEffect(() => {
    if (!isEditing && title !== undefined) {
      setLocalTitle(title);
    }
  }, [title, isEditing]);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const { updatePage } = usePageMutations();

  const handleBlur = useCallback(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(() => {
      setIsEditing(false);
      if (localTitle !== title) {
        updatePage.mutate({ pageId, data: { name: localTitle } });
      }
    }, 800);
  }, [localTitle, title, pageId, updatePage]);

  const handleFocus = useCallback(() => {
    setIsEditing(true);
    if (debounceRef.current) {
      clearTimeout(debounceRef.current);
      debounceRef.current = null;
    }
  }, []);

  if (!isEditing) {
    return (
      <button
        type="button"
        className={`w-full py-3 text-left text-[2rem] leading-[2.375rem] font-bold tracking-[-2%] ${
          !title ? "text-custom-text-400" : "break-words"
        }`}
        onClick={() => setIsEditing(true)}
      >
        {title || "无标题"}
      </button>
    );
  }

  return (
    <textarea
      className="block w-full resize-none rounded-none border-none bg-transparent py-3 text-[2rem] leading-[2.375rem] font-bold tracking-[-2%] outline-none"
      placeholder="无标题"
      value={localTitle}
      onChange={(e) => setLocalTitle(e.target.value)}
      onBlur={handleBlur}
      onFocus={handleFocus}
      maxLength={255}
      rows={1}
    />
  );
};
