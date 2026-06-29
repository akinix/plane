// FLOW: EmojiPicker — searchable emoji grid popover component (per D-P15-10)
import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { ISSUE_REACTION_EMOJI_CODES, RANDOM_EMOJI_CODES } from "@plane/constants";
import { cn } from "../utils";

// ---------------------------------------------------------------------------
// Additional emoji code points to make the picker useful
// ---------------------------------------------------------------------------
const EXTRA_EMOJI_CODES = [
  "127754", "127963", "127961", "127748", "128205", "128200", "128202", "128203",
  "128209", "128218", "128214", "128221", "128224", "128225", "128226", "128227",
  "128228", "128229", "128230", "128231", "128232", "128233", "128234", "128235",
  "128236", "128240", "128241", "128242", "128246", "128247", "128248", "128249",
  "128250", "128251", "128276", "128293", "128295", "128296", "128297", "128298",
  "128299", "128300", "128301", "128302", "128303", "128304", "128305", "128306",
  "128307", "128308", "128309", "128310", "128311", "128312", "128313", "128314",
  "128315", "128316", "128317", "128336", "128337", "129302", "129303", "129304",
  "129305", "129306", "129307", "129308", "129309", "129310", "129311", "129312",
  "129313", "129314", "129315", "129316", "129317", "129318", "129319", "129320",
  "129321", "129322", "129323", "129324", "129325", "129326", "129327", "129328",
  "129329", "129330", "129331", "129332", "129333", "129334", "129335", "129336",
  "129337", "129338", "129339", "129340", "129341", "129342", "129343", "129344",
  "129345", "129346", "129347", "129348", "129349", "129350", "129351", "129352",
  "129353", "129354", "129355", "129356", "129357", "129358", "129359", "129360",
  "129361", "129362", "129363", "129364", "129365", "129366", "129367", "129368",
  "129369", "129370", "129371", "129372", "129373", "129374", "129375", "129376",
  "129377", "129378", "129379", "129380", "129381", "129382", "129383", "129384",
  "129385", "129386", "129387", "129388", "129389", "129390", "129391", "129392",
  "129393", "129394", "129395", "129396", "129397", "129398", "129399", "129400",
  "129401", "129402", "129403", "129404", "129405", "129406", "129407", "129408",
  "129409", "129410", "129411", "129412", "129413", "129414", "129415", "129416",
  "129417", "129418", "129419", "129420", "129421", "129422", "129423", "129424",
  "129425", "129426", "129427", "129428", "129429", "129430", "129431", "129432",
  "129433", "129434", "129435", "129436", "129437", "129438", "129439", "129440",
];

// Combine all emoji codes into a single deduplicated list
const ALL_EMOJI_CODES = Array.from(
  new Set([...ISSUE_REACTION_EMOJI_CODES, ...RANDOM_EMOJI_CODES, ...EXTRA_EMOJI_CODES])
);

const renderEmoji = (code: string): string => {
  try {
    return String.fromCodePoint(parseInt(code, 10));
  } catch {
    return "";
  }
};

// ---------------------------------------------------------------------------
// useOutsideClick
// ---------------------------------------------------------------------------
const useOutsideClick = (ref: React.RefObject<HTMLElement | null>, handler: () => void) => {
  useEffect(() => {
    const listener = (event: MouseEvent | TouchEvent) => {
      if (!ref.current || ref.current.contains(event.target as Node)) return;
      handler();
    };
    document.addEventListener("mousedown", listener);
    document.addEventListener("touchstart", listener);
    return () => {
      document.removeEventListener("mousedown", listener);
      document.removeEventListener("touchstart", listener);
    };
  }, [ref, handler]);
};

// ---------------------------------------------------------------------------
// EmojiPicker
// ---------------------------------------------------------------------------

export type TEmojiPickerProps = {
  value: string | null;
  onChange: (emoji: string) => void;
  label?: React.ReactNode;
  className?: string;
};

export const EmojiPicker: React.FC<TEmojiPickerProps> = (props) => {
  const { value, onChange, label, className } = props;
  const [open, setOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const containerRef = useRef<HTMLDivElement>(null);

  const close = useCallback(() => {
    setOpen(false);
    setSearchQuery("");
  }, []);

  useOutsideClick(containerRef, close);

  const filteredEmojis = useMemo(() => {
    if (!searchQuery.trim()) return ALL_EMOJI_CODES;
    const query = searchQuery.trim().toLowerCase();
    return ALL_EMOJI_CODES.filter((code) => {
      const char = renderEmoji(code);
      return char.toLowerCase().includes(query);
    });
  }, [searchQuery]);

  return (
    <div ref={containerRef} className={cn("relative inline-block", className)}>
      {/* Trigger button */}
      <button
        type="button"
        onClick={() => setOpen((prev) => !prev)}
        className={cn(
          "flex cursor-pointer items-center justify-center rounded border border-border-base px-2 py-1 text-sm transition-colors hover:bg-surface-1",
          "focus:outline-none focus:ring-1 focus:ring-primary"
        )}
      >
        {label ?? (
          <span className="flex items-center gap-1">
            {value ? renderEmoji(value) : "😀"}
            <svg
              className={cn("h-3 w-3 transition-transform", open && "rotate-180")}
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
            </svg>
          </span>
        )}
      </button>

      {/* Popover panel */}
      {open && (
        <div
          className={cn(
            "absolute z-50 mt-1 w-72 rounded-md border border-border-base bg-background shadow-lg",
            "focus:outline-none"
          )}
        >
          <div className="p-2">
            {/* Search input */}
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value.slice(0, 50))}
              placeholder="搜索表情..."
              className="w-full rounded border border-border-base px-2 py-1.5 text-xs placeholder:text-muted focus:border-primary focus:outline-none"
              autoFocus
            />

            {/* Emoji grid */}
            <div className="mt-2 grid max-h-60 grid-cols-8 gap-1 overflow-y-auto">
              {filteredEmojis.map((code) => (
                <button
                  key={code}
                  type="button"
                  title={renderEmoji(code)}
                  onClick={() => {
                    onChange(code);
                    close();
                  }}
                  className={cn(
                    "flex aspect-square items-center justify-center rounded p-1 text-lg transition-colors hover:bg-surface-1",
                    value === code && "bg-primary/10 ring-1 ring-primary"
                  )}
                >
                  {renderEmoji(code)}
                </button>
              ))}
            </div>

            {filteredEmojis.length === 0 && (
              <p className="py-4 text-center text-xs text-muted">未找到匹配的表情</p>
            )}
          </div>
        </div>
      )}
    </div>
  );
};
