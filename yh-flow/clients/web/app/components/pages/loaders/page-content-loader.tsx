// FLOW: Forked from Plane. Original: apps/web/core/components/pages/loaders/page-content-loader.tsx
// FLOW: PageContentLoader — 页面列表加载骨架
"use client";
import { cn } from "@plane/utils";

type Props = {
  className?: string;
};

export function PageContentLoader(props: Props) {
  const { className } = props;

  return (
    <div className={cn("flex size-full animate-pulse flex-col", className)}>
      {/* Tab bar skeleton */}
      <div className="border-custom-border-200 flex h-11 items-center gap-4 border-b px-4">
        <div className="bg-custom-background-80 h-4 w-12 rounded" />
        <div className="bg-custom-background-80 h-4 w-12 rounded" />
        <div className="bg-custom-background-80 h-4 w-16 rounded" />
      </div>

      {/* Card grid skeleton */}
      <div className="grid grid-cols-1 gap-4 px-4 py-4 lg:grid-cols-2 xl:grid-cols-3">
        {[1, 2, 3, 4, 5, 6].map((i) => (
          <div
            key={i}
            className="border-custom-border-200 bg-custom-background-90 flex flex-col gap-3 rounded-lg border p-4"
          >
            <div className="flex items-center gap-3">
              <div className="bg-custom-background-80 size-8 rounded" />
              <div className="bg-custom-background-80 h-4 flex-1 rounded" />
            </div>
            <div className="bg-custom-background-80 h-3 w-2/3 rounded" />
            <div className="bg-custom-background-80 h-3 w-1/3 rounded" />
          </div>
        ))}
      </div>
    </div>
  );
}
