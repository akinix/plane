// FLOW: Forked from Plane. Original: apps/web/core/components/pages/editor/toolbar/toolbar.tsx
// FLOW: PageToolbar — 悬浮工具栏（heading, bold, italic, list, link, quote, code）
"use client";
import { Bold, Italic, Strikethrough, List, ListOrdered, Link, Quote, Code } from "lucide-react";

type ToolbarButtonProps = {
  icon: React.ReactNode;
  onClick: () => void;
  isActive?: boolean;
  label: string;
};

const ToolbarButton = function ToolbarButton({ icon, onClick, isActive, label }: ToolbarButtonProps) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={`grid size-7 shrink-0 place-items-center rounded-sm transition-colors ${
        isActive ? "bg-custom-background-80 text-custom-text-100" : "text-custom-text-400 hover:bg-custom-background-80"
      }`}
      aria-label={label}
      title={label}
    >
      {icon}
    </button>
  );
};

const HEADING_OPTIONS = [
  { label: "标题 1", tag: "h1" },
  { label: "标题 2", tag: "h2" },
  { label: "标题 3", tag: "h3" },
];

type Props = {
  onFormat?: (command: string, value?: string) => void;
};

export const PageToolbar = function PageToolbar({ onFormat }: Props) {
  const handleCommand = (command: string, value?: string) => {
    onFormat?.(command, value);
  };

  return (
    <div className="divide-custom-border-200 flex items-center divide-x overflow-x-auto">
      {/* Heading dropdown */}
      <div className="flex items-center gap-0.5 pr-2">
        <select
          onChange={(e) => handleCommand("heading", e.target.value)}
          className="border-custom-border-200 text-xs text-custom-text-300 h-7 rounded-sm border bg-transparent px-2 outline-none"
          aria-label="标题级别"
        >
          <option value="">正文</option>
          {HEADING_OPTIONS.map((opt) => (
            <option key={opt.tag} value={opt.tag}>
              {opt.label}
            </option>
          ))}
        </select>
      </div>

      {/* Formatting buttons */}
      <div className="flex items-center gap-0.5 px-2">
        <ToolbarButton icon={<Bold className="size-4" />} onClick={() => handleCommand("bold")} label="粗体" />
        <ToolbarButton icon={<Italic className="size-4" />} onClick={() => handleCommand("italic")} label="斜体" />
        <ToolbarButton
          icon={<Strikethrough className="size-4" />}
          onClick={() => handleCommand("strikethrough")}
          label="删除线"
        />
      </div>

      {/* List buttons */}
      <div className="flex items-center gap-0.5 px-2">
        <ToolbarButton
          icon={<List className="size-4" />}
          onClick={() => handleCommand("bulleted-list")}
          label="无序列表"
        />
        <ToolbarButton
          icon={<ListOrdered className="size-4" />}
          onClick={() => handleCommand("numbered-list")}
          label="有序列表"
        />
      </div>

      {/* Block buttons */}
      <div className="flex items-center gap-0.5 px-2">
        <ToolbarButton icon={<Link className="size-4" />} onClick={() => handleCommand("link")} label="链接" />
        <ToolbarButton icon={<Quote className="size-4" />} onClick={() => handleCommand("quote")} label="引用" />
        <ToolbarButton icon={<Code className="size-4" />} onClick={() => handleCommand("code")} label="代码" />
      </div>
    </div>
  );
};
