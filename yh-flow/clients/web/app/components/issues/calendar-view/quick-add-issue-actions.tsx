// FLOW: Forked from Plane calendar/quick-add-issue-actions.tsx
// Adapted: removed @plane/propel/icons/toast, @plane/ui, Next.js router, ExistingIssuesListModal
// Simplified inline creation with a "+" button and inline input
import { useState, useCallback, useEffect, useRef } from "react";
import type { TIssue } from "@plane/types";

type TProps = {
  prePopulatedData?: Partial<TIssue>;
  quickAddCallback?: (projectId: string | null | undefined, data: TIssue) => Promise<TIssue | undefined>;
};

export const CalendarQuickAddIssueActions = function CalendarQuickAddIssueActions(props: TProps) {
  const { prePopulatedData, quickAddCallback } = props;
  const [isOpen, setIsOpen] = useState(false);
  const [title, setTitle] = useState("");
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (isOpen && inputRef.current) {
      inputRef.current.focus();
    }
  }, [isOpen]);

  const handleSubmit = useCallback(async () => {
    if (!title.trim() || !quickAddCallback) return;

    const newIssue: Partial<TIssue> = {
      name: title.trim(),
      ...prePopulatedData,
    };

    await quickAddCallback(null, newIssue as TIssue);
    setTitle("");
    setIsOpen(false);
  }, [title, quickAddCallback, prePopulatedData]);

  if (!isOpen) {
    return (
      <button
        type="button"
        onClick={() => setIsOpen(true)}
        className="text-xs text-custom-text-400 hover:bg-custom-background-80 flex w-full items-center gap-1 rounded-sm px-2 py-1 opacity-0 transition-opacity group-hover:opacity-100"
      >
        <svg className="h-3 w-3" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}>
          <path d="M12 5v14M5 12h14" />
        </svg>
        <span>添加 Issue</span>
      </button>
    );
  }

  return (
    <div className="flex items-center gap-1 px-2 py-1">
      <input
        ref={inputRef}
        type="text"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        onKeyDown={(e) => {
          if (e.key === "Enter") handleSubmit();
          if (e.key === "Escape") {
            setIsOpen(false);
            setTitle("");
          }
        }}
        placeholder="Issue 标题..."
        className="border-custom-border-200 bg-custom-background-100 text-xs w-full rounded-sm border px-2 py-1 outline-none"
      />
      <button
        type="button"
        onClick={handleSubmit}
        disabled={!title.trim()}
        className="bg-custom-primary text-xs rounded-sm px-2 py-1 text-white disabled:opacity-50"
      >
        添加
      </button>
    </div>
  );
};
