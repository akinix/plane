// FLOW: Forked from Plane. Original: apps/web/core/components/views/view-list-item-action.tsx
// FLOW: ViewListItemAction — 视图列表项操作（编辑 / 删除下拉菜单）
"use client";
import { useState, useRef, useEffect } from "react";
import { MoreHorizontal, Pencil, Trash2 } from "lucide-react";
import type { TIssueView } from "@/components/issues/filters/types";

type Props = {
  view: TIssueView;
  projectId: string;
  onEdit: () => void;
  onDelete: () => void;
};

export function ViewListItemAction({ view: _view, projectId: _projectId, onEdit, onDelete }: Props) {
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setMenuOpen(false);
      }
    };
    if (menuOpen) {
      document.addEventListener("mousedown", handleClickOutside);
    }
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [menuOpen]);

  return (
    <div ref={menuRef} className="relative flex-shrink-0">
      <button
        type="button"
        onClick={(e) => {
          e.stopPropagation();
          setMenuOpen(!menuOpen);
        }}
        className="text-custom-text-400 hover:text-custom-text-200 rounded p-1"
      >
        <MoreHorizontal className="size-4" />
      </button>

      {menuOpen && (
        <div className="shadow-custom-shadow-2xs border-custom-border-200 bg-custom-background-90 absolute right-0 top-full z-10 mt-1 min-w-[140px] rounded-md border p-1">
          <button
            type="button"
            onClick={(e) => {
              e.stopPropagation();
              setMenuOpen(false);
              onEdit();
            }}
            className="hover:bg-custom-background-80 text-custom-text-200 hover:text-custom-text-100 flex w-full items-center gap-2 rounded-sm px-2 py-1.5 text-xs"
          >
            <Pencil className="size-3.5" />
            编辑
          </button>
          <button
            type="button"
            onClick={(e) => {
              e.stopPropagation();
              setMenuOpen(false);
              onDelete();
            }}
            className="hover:bg-custom-background-80 text-red-500 flex w-full items-center gap-2 rounded-sm px-2 py-1.5 text-xs"
          >
            <Trash2 className="size-3.5" />
            删除
          </button>
        </div>
      )}
    </div>
  );
}

