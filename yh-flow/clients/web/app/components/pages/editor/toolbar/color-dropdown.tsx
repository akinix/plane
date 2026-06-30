// FLOW: Forked from Plane. Original: apps/web/core/components/pages/editor/toolbar/color-dropdown.tsx
// FLOW: ColorDropdown — 文字颜色选择器
"use client";
import { useState } from "react";
import { ALargeSmall, Ban } from "lucide-react";
import { cn } from "@plane/utils";

const PRESET_COLORS = [
  { key: "red", textColor: "#EF4444", backgroundColor: "#FEE2E2" },
  { key: "orange", textColor: "#F97316", backgroundColor: "#FFEDD5" },
  { key: "yellow", textColor: "#EAB308", backgroundColor: "#FEF9C3" },
  { key: "green", textColor: "#22C55E", backgroundColor: "#DCFCE7" },
  { key: "blue", textColor: "#3B82F6", backgroundColor: "#DBEAFE" },
  { key: "purple", textColor: "#A855F7", backgroundColor: "#F3E8FF" },
  { key: "pink", textColor: "#EC4899", backgroundColor: "#FCE7F3" },
  { key: "gray", textColor: "#6B7280", backgroundColor: "#F3F4F6" },
];

export const ColorDropdown = function ColorDropdown() {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <div className="relative h-7 px-2">
      <button
        type="button"
        onClick={() => setIsOpen(!isOpen)}
        className={cn(
          "text-xs flex h-7 items-center gap-1.5 rounded-sm px-2 outline-none",
          "text-custom-text-400 hover:bg-custom-background-80",
          { "bg-custom-background-80": isOpen }
        )}
      >
        Color
        <span className="border-custom-border-200 bg-custom-background-100 grid size-6 shrink-0 place-items-center rounded-sm border">
          <ALargeSmall className="text-custom-text-400 size-3.5" />
        </span>
      </button>

      {isOpen && (
        <div className="border-custom-border-200 bg-custom-background-100 shadow-lg absolute z-20 mt-1 space-y-2 rounded-md border p-2">
          <div className="space-y-1.5">
            <p className="text-xs text-custom-text-400 font-semibold">文字颜色</p>
            <div className="flex items-center gap-2">
              {PRESET_COLORS.map((color) => (
                <button
                  key={color.key}
                  type="button"
                  className="border-custom-border-200 size-6 flex-shrink-0 rounded-sm border transition-opacity hover:opacity-60"
                  style={{ backgroundColor: color.textColor }}
                  aria-label={color.key}
                />
              ))}
              <button
                type="button"
                className="border-custom-border-200 text-custom-text-400 hover:bg-custom-background-80 grid size-6 flex-shrink-0 place-items-center rounded-sm border transition-colors"
                aria-label="清除颜色"
              >
                <Ban className="size-4" />
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
