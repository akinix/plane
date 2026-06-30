// FLOW: Forked from Plane. Original: apps/web/core/components/pages/loaders/page-loader.tsx
// FLOW: PageLoader — 编辑器全页加载骨架屏
"use client";

const SKELETON_ROWS = [
  { width: "240px" },
  { width: "320px" },
  { width: "180px" },
  { width: "360px" },
  { width: "280px" },
  { width: "200px" },
  { width: "340px" },
  { width: "260px" },
];

export function PageLoader() {
  return (
    <div className="relative flex size-full flex-col">
      {/* Header skeleton */}
      <div className="border-custom-border-200 border-b px-6 py-3">
        <div className="flex items-center gap-2">
          <div className="bg-custom-background-80 h-8 w-[200px] animate-pulse rounded" />
          <div className="ml-auto flex items-center gap-2">
            <div className="bg-custom-background-80 h-8 w-[100px] animate-pulse rounded" />
            <div className="bg-custom-background-80 h-8 w-[100px] animate-pulse rounded" />
            <div className="bg-custom-background-80 h-8 w-[100px] animate-pulse rounded" />
          </div>
        </div>
      </div>
      {/* Title skeleton */}
      <div className="px-page-x py-6">
        <div className="bg-custom-background-80 h-10 w-[300px] animate-pulse rounded" />
      </div>
      {/* Content skeleton */}
      <div className="space-y-4 px-page-x">
        {SKELETON_ROWS.map((row) => (
          <div key={row.width} className="flex items-center gap-2">
            <div className="bg-custom-background-80 h-5 animate-pulse rounded" style={{ width: row.width }} />
            <div className="ml-auto flex items-center gap-2">
              <div className="bg-custom-background-80 h-5 w-[60px] animate-pulse rounded" />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
