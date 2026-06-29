// FLOW: PropertyEditor — reusable inline property editor components per D-P16-10
// Each sub-component: click to expand inline dropdown/popup editor, select to save
"use client";

import { useState, useRef, useEffect } from "react";
import type { TIssuePriorities, IState } from "@plane/types";
import { cn } from "@plane/utils";
import { Avatar } from "@plane/ui";
import { MOCK_STATES, MOCK_LABELS, MOCK_MEMBERS } from "@/../src/lib/mock-data";

// ---------------------------------------------------------------------------
// Shared helpers
// ---------------------------------------------------------------------------

const PRIORITY_CONFIG: Record<TIssuePriorities | "none", { label: string; color: string }> = {
  urgent: { label: "紧急", color: "#D1453B" },
  high: { label: "高", color: "#D97706" },
  medium: { label: "中", color: "#EAB308" },
  low: { label: "低", color: "#3B82F6" },
  none: { label: "无", color: "#9CA3AF" },
};

function useOutsideClick(ref: React.RefObject<HTMLElement | null>, handler: () => void) {
  useEffect(() => {
    function handleClick(event: MouseEvent) {
      if (ref.current && !ref.current.contains(event.target as Node)) {
        handler();
      }
    }
    document.addEventListener("mousedown", handleClick);
    return () => document.removeEventListener("mousedown", handleClick);
  }, [ref, handler]);
}

function InlineEditor({
  children,
  isOpen,
  onToggle,
}: {
  children: React.ReactNode;
  isOpen: boolean;
  onToggle: () => void;
}) {
  const ref = useRef<HTMLDivElement>(null);
  useOutsideClick(ref, () => {
    if (isOpen) onToggle();
  });

  return (
    <div ref={ref} className="relative">
      {children}
    </div>
  );
}

// ---------------------------------------------------------------------------
// PropertyEditorState
// ---------------------------------------------------------------------------

type StateProps = {
  value: string | null;
  projectId: string;
  onSelect: (stateId: string) => void;
};

export function PropertyEditorState({ value, projectId, onSelect }: StateProps) {
  const [open, setOpen] = useState(false);
  const states = MOCK_STATES.filter((s) => s.project_id === projectId);
  const current = states.find((s) => s.id === value);

  const grouped = states.reduce<Record<string, IState[]>>((acc, s) => {
    if (!acc[s.group]) acc[s.group] = [];
    acc[s.group].push(s);
    return acc;
  }, {});

  return (
    <InlineEditor isOpen={open} onToggle={() => setOpen(!open)}>
      <button
        onClick={() => setOpen(!open)}
        className="text-sm hover:bg-custom-background-80 flex w-full items-center gap-2 rounded-md px-2 py-1 text-left transition-colors"
      >
        {current ? (
          <>
            <span className="size-3 flex-shrink-0 rounded-full" style={{ backgroundColor: current.color }} />
            <span>{current.name}</span>
          </>
        ) : (
          <span className="text-custom-text-400">未设置</span>
        )}
      </button>
      {open && (
        <div className="border-custom-border-200 bg-custom-background-90 shadow-lg absolute top-full left-0 z-20 mt-1 min-w-[180px] rounded-md border p-1">
          {Object.entries(grouped).map(([group, groupStates]) => (
            <div key={group}>
              <div className="text-xs text-custom-text-400 px-2 py-1 capitalize">{group}</div>
              {groupStates.map((s) => (
                <button
                  key={s.id}
                  onClick={() => {
                    onSelect(s.id);
                    setOpen(false);
                  }}
                  className={cn(
                    "text-sm hover:bg-custom-background-80 flex w-full items-center gap-2 rounded-sm px-2 py-1.5 transition-colors",
                    s.id === value && "bg-custom-background-80"
                  )}
                >
                  <span className="size-3 flex-shrink-0 rounded-full" style={{ backgroundColor: s.color }} />
                  <span>{s.name}</span>
                </button>
              ))}
            </div>
          ))}
        </div>
      )}
    </InlineEditor>
  );
}

// ---------------------------------------------------------------------------
// PropertyEditorPriority
// ---------------------------------------------------------------------------

type PriorityProps = {
  value: TIssuePriorities | null;
  onSelect: (priority: TIssuePriorities) => void;
};

export function PropertyEditorPriority({ value, onSelect }: PriorityProps) {
  const [open, setOpen] = useState(false);
  const priorities: TIssuePriorities[] = ["urgent", "high", "medium", "low", "none"];

  return (
    <InlineEditor isOpen={open} onToggle={() => setOpen(!open)}>
      <button
        onClick={() => setOpen(!open)}
        className="text-sm hover:bg-custom-background-80 flex w-full items-center gap-2 rounded-md px-2 py-1 text-left transition-colors"
      >
        {value && value !== "none" ? (
          <>
            <span
              className="size-3 flex-shrink-0 rounded-full"
              style={{ backgroundColor: PRIORITY_CONFIG[value].color }}
            />
            <span>{PRIORITY_CONFIG[value].label}</span>
          </>
        ) : (
          <span className="text-custom-text-400">无</span>
        )}
      </button>
      {open && (
        <div className="border-custom-border-200 bg-custom-background-90 shadow-lg absolute top-full left-0 z-20 mt-1 min-w-[160px] rounded-md border p-1">
          {priorities.map((p) => (
            <button
              key={p}
              onClick={() => {
                onSelect(p);
                setOpen(false);
              }}
              className={cn(
                "text-sm hover:bg-custom-background-80 flex w-full items-center gap-2 rounded-sm px-2 py-1.5 transition-colors",
                p === value && "bg-custom-background-80"
              )}
            >
              <span
                className="size-3 flex-shrink-0 rounded-full"
                style={{ backgroundColor: PRIORITY_CONFIG[p].color }}
              />
              <span>{PRIORITY_CONFIG[p].label}</span>
            </button>
          ))}
        </div>
      )}
    </InlineEditor>
  );
}

// ---------------------------------------------------------------------------
// PropertyEditorAssignee
// ---------------------------------------------------------------------------

type AssigneeProps = {
  value: string[];
  workspaceId: string;
  onSelect: (assigneeIds: string[]) => void;
};

export function PropertyEditorAssignee({ value, workspaceId, onSelect }: AssigneeProps) {
  const [open, setOpen] = useState(false);
  const members = MOCK_MEMBERS[workspaceId] ?? [];

  return (
    <InlineEditor isOpen={open} onToggle={() => setOpen(!open)}>
      <button
        onClick={() => setOpen(!open)}
        className="text-sm hover:bg-custom-background-80 flex w-full items-center gap-2 rounded-md px-2 py-1 text-left transition-colors"
      >
        {value.length > 0 ? (
          <div className="flex -space-x-1">
            {value.map((userId) => {
              const member = members.find((m) => m.member.id === userId);
              return (
                <Avatar
                  key={userId}
                  name={member?.member.display_name ?? userId}
                  size="sm"
                  className="border-custom-border-200 border"
                />
              );
            })}
          </div>
        ) : (
          <span className="text-custom-text-400">未指派</span>
        )}
      </button>
      {open && (
        <div className="border-custom-border-200 bg-custom-background-90 shadow-lg absolute top-full left-0 z-20 mt-1 max-h-48 min-w-[180px] overflow-y-auto rounded-md border p-1">
          {members.map((m) => (
            <button
              key={m.id}
              onClick={() => onSelect([m.member.id])}
              className={cn(
                "text-sm hover:bg-custom-background-80 flex w-full items-center gap-2 rounded-sm px-2 py-1.5 transition-colors",
                value.includes(m.member.id) && "bg-custom-background-80"
              )}
            >
              <Avatar name={m.member.display_name} size="sm" />
              <span>{m.member.display_name}</span>
            </button>
          ))}
        </div>
      )}
    </InlineEditor>
  );
}

// ---------------------------------------------------------------------------
// PropertyEditorLabels
// ---------------------------------------------------------------------------

type LabelsProps = {
  value: string[];
  projectId: string;
  onSelect: (labelIds: string[]) => void;
};

export function PropertyEditorLabels({ value, projectId, onSelect }: LabelsProps) {
  const [open, setOpen] = useState(false);
  const labels = MOCK_LABELS.filter((l) => l.project_id === projectId);

  const toggleLabel = (labelId: string) => {
    const newValue = value.includes(labelId) ? value.filter((id) => id !== labelId) : [...value, labelId];
    onSelect(newValue);
  };

  return (
    <InlineEditor isOpen={open} onToggle={() => setOpen(!open)}>
      <button
        onClick={() => setOpen(!open)}
        className="text-sm hover:bg-custom-background-80 flex w-full flex-wrap items-center gap-1 rounded-md px-2 py-1 text-left transition-colors"
      >
        {value.length > 0 ? (
          value.map((labelId) => {
            const label = labels.find((l) => l.id === labelId);
            return label ? (
              <span
                key={labelId}
                className="text-xs inline-flex items-center gap-1 rounded-sm px-1.5 py-0.5"
                style={{ backgroundColor: `${label.color}20`, color: label.color }}
              >
                {label.name}
              </span>
            ) : null;
          })
        ) : (
          <span className="text-custom-text-400">未设置</span>
        )}
      </button>
      {open && (
        <div className="border-custom-border-200 bg-custom-background-90 shadow-lg absolute top-full left-0 z-20 mt-1 max-h-48 min-w-[180px] overflow-y-auto rounded-md border p-1">
          {labels.map((l) => (
            <button
              key={l.id}
              onClick={() => toggleLabel(l.id)}
              className={cn(
                "text-sm hover:bg-custom-background-80 flex w-full items-center gap-2 rounded-sm px-2 py-1.5 transition-colors",
                value.includes(l.id) && "bg-custom-background-80"
              )}
            >
              <span className="size-3 flex-shrink-0 rounded-sm" style={{ backgroundColor: l.color }} />
              <span>{l.name}</span>
              {value.includes(l.id) && <span className="text-custom-text-400 text-xs ml-auto">&#x2713;</span>}
            </button>
          ))}
        </div>
      )}
    </InlineEditor>
  );
}

// ---------------------------------------------------------------------------
// PropertyEditorEstimate
// ---------------------------------------------------------------------------

type EstimateProps = {
  value: string | null;
  onSelect: (estimate: string | null) => void;
};

export function PropertyEditorEstimate({ value, onSelect }: EstimateProps) {
  const [open, setOpen] = useState(false);
  const [inputValue, setInputValue] = useState(value ?? "");

  const quickOptions = ["0", "1", "2", "3", "5", "8", "13"];

  return (
    <InlineEditor isOpen={open} onToggle={() => setOpen(!open)}>
      <button
        onClick={() => {
          setOpen(!open);
          setInputValue(value ?? "");
        }}
        className="text-sm hover:bg-custom-background-80 flex w-full items-center gap-2 rounded-md px-2 py-1 text-left transition-colors"
      >
        {value ? <span>{value} 点</span> : <span className="text-custom-text-400">未估算</span>}
      </button>
      {open && (
        <div className="border-custom-border-200 bg-custom-background-90 shadow-lg absolute top-full left-0 z-20 mt-1 min-w-[160px] rounded-md border p-2">
          <input
            type="number"
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            className="border-custom-border-200 bg-custom-background-100 text-sm focus:border-custom-primary w-full rounded-sm border px-2 py-1 outline-none"
            placeholder="输入点数"
          />
          <div className="mt-2 flex flex-wrap gap-1">
            {quickOptions.map((opt) => (
              <button
                key={opt}
                onClick={() => {
                  onSelect(opt);
                  setOpen(false);
                }}
                className="text-xs bg-custom-background-80 hover:bg-custom-background-70 rounded-sm px-2 py-1 transition-colors"
              >
                {opt}
              </button>
            ))}
            {value && (
              <button
                onClick={() => {
                  onSelect(null);
                  setOpen(false);
                }}
                className="text-xs text-custom-text-400 hover:text-custom-text-300 rounded-sm px-2 py-1 transition-colors"
              >
                清除
              </button>
            )}
          </div>
          <button
            onClick={() => {
              onSelect(inputValue || null);
              setOpen(false);
            }}
            className="bg-custom-primary text-xs mt-2 w-full rounded-sm px-2 py-1 text-white"
          >
            确认
          </button>
        </div>
      )}
    </InlineEditor>
  );
}

// ---------------------------------------------------------------------------
// PropertyEditorDate
// ---------------------------------------------------------------------------

type DateProps = {
  value: string | null;
  onSelect: (date: string | null) => void;
};

export function PropertyEditorDate({ value, onSelect }: DateProps) {
  const [open, setOpen] = useState(false);

  const formattedDate = value
    ? (() => {
        try {
          return new Date(value).toISOString().split("T")[0];
        } catch {
          return "";
        }
      })()
    : "";

  return (
    <InlineEditor isOpen={open} onToggle={() => setOpen(!open)}>
      <button
        onClick={() => setOpen(!open)}
        className="text-sm hover:bg-custom-background-80 flex w-full items-center gap-2 rounded-md px-2 py-1 text-left transition-colors"
      >
        {formattedDate ? <span>{formattedDate}</span> : <span className="text-custom-text-400">未设置</span>}
      </button>
      {open && (
        <div className="border-custom-border-200 bg-custom-background-90 shadow-lg absolute top-full left-0 z-20 mt-1 min-w-[200px] rounded-md border p-2">
          <input
            type="date"
            value={formattedDate}
            onChange={(e) => {
              if (e.target.value) {
                onSelect(new Date(e.target.value).toISOString());
                setOpen(false);
              }
            }}
            className="border-custom-border-200 bg-custom-background-100 text-sm focus:border-custom-primary w-full rounded-sm border px-2 py-1 outline-none"
          />
          {value && (
            <button
              onClick={() => {
                onSelect(null);
                setOpen(false);
              }}
              className="text-xs text-custom-text-400 hover:text-custom-text-300 mt-1 w-full rounded-sm px-2 py-1 transition-colors"
            >
              清除日期
            </button>
          )}
        </div>
      )}
    </InlineEditor>
  );
}
