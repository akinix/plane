// FLOW: Forked from Plane. Original: apps/web/core/components/pages/header/copy-link-control.tsx
// FLOW: PageCopyLinkControl — 复制链接按钮
"use client";
import { useState, useRef, useCallback, useEffect } from "react";
import { Link, Check } from "lucide-react";
import { cn } from "@plane/utils";

export const PageCopyLinkControl = function PageCopyLinkControl() {
  const [isCopied, setIsCopied] = useState(false);
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    return () => {
      if (timerRef.current) clearTimeout(timerRef.current);
    };
  }, []);

  const handleCopy = useCallback(() => {
    navigator.clipboard.writeText(window.location.href);
    setIsCopied(true);
    if (timerRef.current) clearTimeout(timerRef.current);
    timerRef.current = setTimeout(() => {
      setIsCopied(false);
    }, 1000);
  }, []);

  return (
    <button
      type="button"
      onClick={handleCopy}
      className={cn(
        "grid size-7 place-items-center rounded transition-colors",
        "hover:bg-custom-background-80",
        isCopied && "text-green-500"
      )}
      aria-label={isCopied ? "已复制链接" : "复制链接"}
    >
      {isCopied ? <Check className="size-4" /> : <Link className="size-4" />}
    </button>
  );
};
