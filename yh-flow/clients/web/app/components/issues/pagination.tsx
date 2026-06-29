// FLOW: Pagination component for Issue list (per D-P16-07)
import { ChevronLeft, ChevronRight } from "lucide-react";
import { cn } from "@plane/utils";

type TProps = {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
};

const getVisiblePages = (current: number, total: number): (number | "...")[] => {
  if (total <= 7) {
    return Array.from({ length: total }, (_, i) => i + 1);
  }

  const pages: (number | "...")[] = [];

  if (current <= 4) {
    for (let i = 1; i <= 5; i++) pages.push(i);
    pages.push("...");
    pages.push(total);
  } else if (current >= total - 3) {
    pages.push(1);
    pages.push("...");
    for (let i = total - 4; i <= total; i++) pages.push(i);
  } else {
    pages.push(1);
    pages.push("...");
    for (let i = current - 1; i <= current + 1; i++) pages.push(i);
    pages.push("...");
    pages.push(total);
  }

  return pages;
};

export const Pagination = ({ currentPage, totalPages, onPageChange }: TProps) => {
  if (totalPages <= 1) return null;

  const visiblePages = getVisiblePages(currentPage, totalPages);

  const btnBase =
    "flex size-8 items-center justify-center rounded-md text-xs transition-colors";

  return (
    <div className="flex items-center gap-1">
      {/* First page */}
      <button
        type="button"
        disabled={currentPage <= 1}
        onClick={() => onPageChange(1)}
        className={cn(
          btnBase,
          "text-custom-text-300 hover:bg-custom-background-80",
          currentPage <= 1 && "cursor-not-allowed opacity-50"
        )}
      >
        <ChevronLeft className="size-3.5" />
        <ChevronLeft className="-ml-1 size-3.5" />
      </button>

      {/* Previous */}
      <button
        type="button"
        disabled={currentPage <= 1}
        onClick={() => onPageChange(currentPage - 1)}
        className={cn(
          btnBase,
          "text-custom-text-300 hover:bg-custom-background-80",
          currentPage <= 1 && "cursor-not-allowed opacity-50"
        )}
      >
        <ChevronLeft className="size-3.5" />
      </button>

      {/* Page numbers */}
      {visiblePages.map((page, idx) =>
        page === "..." ? (
          <span key={`ellipsis-${idx}`} className="flex size-8 items-center justify-center text-xs text-custom-text-300">
            ...
          </span>
        ) : (
          <button
            key={page}
            type="button"
            onClick={() => onPageChange(page)}
            className={cn(
              btnBase,
              "font-medium",
              page === currentPage
                ? "bg-custom-primary text-white"
                : "text-custom-text-300 hover:bg-custom-background-80"
            )}
          >
            {page}
          </button>
        )
      )}

      {/* Next */}
      <button
        type="button"
        disabled={currentPage >= totalPages}
        onClick={() => onPageChange(currentPage + 1)}
        className={cn(
          btnBase,
          "text-custom-text-300 hover:bg-custom-background-80",
          currentPage >= totalPages && "cursor-not-allowed opacity-50"
        )}
      >
        <ChevronRight className="size-3.5" />
      </button>

      {/* Last page */}
      <button
        type="button"
        disabled={currentPage >= totalPages}
        onClick={() => onPageChange(totalPages)}
        className={cn(
          btnBase,
          "text-custom-text-300 hover:bg-custom-background-80",
          currentPage >= totalPages && "cursor-not-allowed opacity-50"
        )}
      >
        <ChevronRight className="size-3.5" />
        <ChevronRight className="-ml-1 size-3.5" />
      </button>
    </div>
  );
};
