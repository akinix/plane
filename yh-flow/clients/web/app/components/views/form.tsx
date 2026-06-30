// FLOW: Forked from Plane. Original: apps/web/core/components/views/form.tsx
// FLOW: ViewForm — 视图创建/编辑表单（名称输入 + 描述 + 访问权限切换）
"use client";
import { useState } from "react";
import { X } from "lucide-react";
import { EViewAccess } from "@/../src/lib/types/views";

type Props = {
  title: string;
  defaultName?: string;
  onSubmit: (data: { name: string; access: EViewAccess; description: string }) => Promise<void>;
  onCancel: () => void;
  isPending?: boolean;
};

const ViewForm = function ViewForm({ title, defaultName = "", onSubmit, onCancel, isPending = false }: Props) {
  const [name, setName] = useState(defaultName);
  const [access, setAccess] = useState<EViewAccess>(EViewAccess.PUBLIC);
  const [description, setDescription] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;
    await onSubmit({ name: name.trim(), access, description });
  };

  return (
    <form onSubmit={handleSubmit} className="flex flex-col">
      {/* Header */}
      <div className="border-custom-border-200 flex items-center justify-between border-b px-5 py-4">
        <span className="text-base font-semibold text-custom-text-100">{title}</span>
        <button type="button" onClick={onCancel} className="text-custom-text-400 hover:text-custom-text-200">
          <X className="size-4" />
        </button>
      </div>

      {/* Body */}
      <div className="flex flex-col gap-4 px-5 py-4">
        {/* Name input */}
        <div className="flex flex-col gap-1.5">
          <label htmlFor="view-name" className="text-xs font-medium text-custom-text-300">
            视图名称
          </label>
          <input
            id="view-name"
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="输入视图名称..."
            className="border-custom-border-200 bg-custom-background-90 text-sm text-custom-text-100 placeholder:text-custom-text-400 focus:border-custom-primary rounded-md border px-3 py-2 outline-none"
          />
        </div>

        {/* Description input */}
        <div className="flex flex-col gap-1.5">
          <label htmlFor="view-description" className="text-xs font-medium text-custom-text-300">
            描述
          </label>
          <input
            id="view-description"
            type="text"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="添加描述（可选）"
            className="border-custom-border-200 bg-custom-background-90 text-sm text-custom-text-100 placeholder:text-custom-text-400 focus:border-custom-primary rounded-md border px-3 py-2 outline-none"
          />
        </div>

        {/* Access toggle */}
        <div className="flex flex-col gap-1.5">
          <span className="text-xs font-medium text-custom-text-300">访问权限</span>
          <div className="flex gap-2">
            <button
              type="button"
              onClick={() => setAccess(EViewAccess.PUBLIC)}
              className={`flex items-center gap-2 rounded-md border px-3 py-2 text-xs font-medium ${
                access === EViewAccess.PUBLIC
                  ? "border-custom-primary text-custom-primary bg-custom-primary/10"
                  : "border-custom-border-200 text-custom-text-300"
              }`}
            >
              公开
            </button>
            <button
              type="button"
              onClick={() => setAccess(EViewAccess.PRIVATE)}
              className={`flex items-center gap-2 rounded-md border px-3 py-2 text-xs font-medium ${
                access === EViewAccess.PRIVATE
                  ? "border-custom-primary text-custom-primary bg-custom-primary/10"
                  : "border-custom-border-200 text-custom-text-300"
              }`}
            >
              私人
            </button>
          </div>
        </div>
      </div>

      {/* Footer */}
      <div className="border-custom-border-200 flex items-center justify-end gap-2 border-t px-5 py-4">
        <button
          type="button"
          onClick={onCancel}
          className="border-custom-border-200 text-custom-text-200 hover:bg-custom-background-80 rounded-md border px-4 py-2 text-xs font-medium"
        >
          取消
        </button>
        <button
          type="submit"
          disabled={!name.trim() || isPending}
          className="bg-custom-primary hover:bg-custom-primary/90 rounded-md px-4 py-2 text-xs font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
        >
          {isPending ? "保存中..." : "保存"}
        </button>
      </div>
    </form>
  );
};

export { ViewForm };
