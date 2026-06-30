// FLOW: Forked from Plane. Original: apps/web/core/components/pages/editor/header/logo-picker.tsx
// FLOW: PageEditorHeaderLogoPicker — Logo 选择器
"use client";
import { useState } from "react";
import { SmilePlus } from "lucide-react";
import { cn } from "@plane/utils";
import type { TPage } from "@plane/types";

type Props = {
  page: TPage;
};

export const PageEditorHeaderLogoPicker = function PageEditorHeaderLogoPicker({ page }: Props) {
  const [isOpen, _setIsOpen] = useState(false);
  const hasLogo = page.logo_props?.in_use;

  const handleLogoClick = () => {
    // Placeholder: emoji picker not yet implemented
  };

  if (!hasLogo) {
    return (
      <button
        type="button"
        onClick={handleLogoClick}
        className={cn(
          "text-xs text-custom-text-400 flex items-center gap-1 rounded-sm p-1 transition-colors outline-none",
          "hover:bg-custom-background-80",
          { "bg-custom-background-80": isOpen }
        )}
      >
        <SmilePlus className="size-4 flex-shrink-0" />
        Icon
      </button>
    );
  }

  return (
    <div className="flex items-center justify-center">
      <span className="text-3xl">{page.logo_props?.emoji?.value || "📄"}</span>
    </div>
  );
};
