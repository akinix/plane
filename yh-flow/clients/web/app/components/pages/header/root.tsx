// FLOW: Forked from Plane. Original: apps/web/core/components/pages/header/root.tsx
// FLOW: PagesListHeaderRoot — 页面列表 Header 容器
"use client";
import type { ReactNode } from "react";

type Props = {
  children?: ReactNode;
};

export const PagesListHeaderRoot = function PagesListHeaderRoot({ children }: Props) {
  return <div className="flex items-center justify-between px-4 py-2">{children}</div>;
};
