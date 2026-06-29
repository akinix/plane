// FLOW: Forked from Plane cycles/analytics-sidebar/sidebar-header.tsx
import React from "react";
import { X } from "lucide-react";

type Props = {
  handleClose: () => void;
};

export const CycleSidebarHeader: React.FC<Props> = ({ handleClose }) => {
  return (
    <div className="flex items-center justify-between px-5 pt-4">
      <h2 className="text-sm font-semibold text-primary">周期统计</h2>
      <button
        type="button"
        className="flex items-center justify-center rounded p-1 text-tertiary hover:bg-surface-2 hover:text-primary transition-colors"
        onClick={handleClose}
        aria-label="关闭"
      >
        <X className="h-4 w-4" />
      </button>
    </div>
  );
};
