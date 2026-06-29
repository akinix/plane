// FLOW: Forked from Plane cycles/dropdowns/estimate-type-dropdown.tsx
import React from "react";
import { ChevronDown } from "lucide-react";

type EstimateType = "issues" | "points";

type Props = {
  value: EstimateType;
  onChange: (type: EstimateType) => void;
};

export const EstimateTypeDropdown: React.FC<Props> = ({ value, onChange }) => {
  const [isOpen, setIsOpen] = React.useState(false);

  const options: { key: EstimateType; label: string }[] = [
    { key: "issues", label: "Issue 计数" },
    { key: "points", label: "Story Points" },
  ];

  const currentLabel = options.find((o) => o.key === value)?.label ?? "Issue 计数";

  return (
    <div className="relative">
      <button
        type="button"
        className="flex items-center gap-1 rounded border border-subtle bg-surface-1 px-2 py-1 text-xs text-secondary hover:bg-surface-2 transition-colors"
        onClick={() => setIsOpen(!isOpen)}
      >
        {currentLabel}
        <ChevronDown className="h-3 w-3" />
      </button>
      {isOpen && (
        <>
          <div className="fixed inset-0 z-10" onClick={() => setIsOpen(false)} />
          <div className="absolute right-0 z-20 mt-1 min-w-[140px] rounded-md border border-subtle bg-surface-1 shadow-lg">
            {options.map((opt) => (
              <button
                key={opt.key}
                type="button"
                className={`flex w-full items-center px-3 py-2 text-xs hover:bg-surface-2 transition-colors ${
                  value === opt.key ? "text-accent font-medium" : "text-secondary"
                }`}
                onClick={() => {
                  onChange(opt.key);
                  setIsOpen(false);
                }}
              >
                {opt.label}
              </button>
            ))}
          </div>
        </>
      )}
    </div>
  );
};
