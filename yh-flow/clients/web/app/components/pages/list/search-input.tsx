// FLOW: Forked from Plane. Original: apps/web/core/components/pages/list/search-input.tsx
// FLOW: PageSearchInput — 搜索输入框
"use client";
import { useRef, useState } from "react";
import { Search, X } from "lucide-react";
import { cn } from "@plane/utils";

type Props = {
  searchQuery: string;
  updateSearchQuery: (val: string) => void;
};

export function PageSearchInput(props: Props) {
  const { searchQuery, updateSearchQuery } = props;
  const [isSearchOpen, setIsSearchOpen] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  const handleInputKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Escape") {
      if (searchQuery && searchQuery.trim() !== "") updateSearchQuery("");
      else {
        setIsSearchOpen(false);
        inputRef.current?.blur();
      }
    }
  };

  return (
    <div className="flex">
      {!isSearchOpen && (
        <button
          className="text-custom-text-300 hover:text-custom-text-200 my-auto grid shrink-0 place-items-center p-1"
          onClick={() => {
            setIsSearchOpen(true);
            setTimeout(() => inputRef.current?.focus(), 0);
          }}
        >
          <Search className="size-3.5" />
        </button>
      )}
      <div
        className={cn(
          "text-custom-text-400 flex items-center justify-start overflow-hidden rounded-md border border-transparent opacity-0 transition-[width] ease-linear",
          {
            "border-custom-border-200 bg-custom-background-80 w-64 px-2.5 py-1.5 opacity-100": isSearchOpen,
          }
        )}
      >
        <Search className="size-3.5 flex-shrink-0" />
        <input
          ref={inputRef}
          className="text-sm text-custom-text-100 placeholder:text-custom-text-400 ml-2 w-full max-w-[234px] border-none bg-transparent focus:outline-none"
          placeholder="搜索页面..."
          value={searchQuery}
          onChange={(e) => updateSearchQuery(e.target.value)}
          onKeyDown={handleInputKeyDown}
        />
        {isSearchOpen && (
          <button
            type="button"
            className="text-custom-text-300 hover:text-custom-text-200 grid place-items-center"
            onClick={() => {
              updateSearchQuery("");
              setIsSearchOpen(false);
            }}
          >
            <X className="size-3" />
          </button>
        )}
      </div>
    </div>
  );
}
