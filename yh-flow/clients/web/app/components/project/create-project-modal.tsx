// FLOW: CreateProjectModal — modal dialog for creating a project (PROJ-01)
import { useState, type FormEvent } from "react";
import { X } from "lucide-react";
import { useNavigate } from "react-router";
import { EmojiPicker } from "../../../src/lib/ui/emoji-picker";

type TProps = {
  isOpen: boolean;
  onClose: () => void;
  workspaceId: string;
};

export const CreateProjectModal = ({
  isOpen,
  onClose,
  workspaceId,
}: TProps) => {
  const navigate = useNavigate();
  const [name, setName] = useState("");
  const [identifier, setIdentifier] = useState("");
  const [description, setDescription] = useState("");
  const [emoji, setEmoji] = useState<string | null>(null);

  if (!isOpen) return null;

  const autoIdentifier = name
    .split(" ")
    .map((w) => w.charAt(0))
    .join("")
    .toUpperCase()
    .slice(0, 5);

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    const newProject = {
      name,
      identifier: identifier || autoIdentifier,
      description,
      emoji,
      workspaceId,
    };
    console.log("Project created:", newProject);
    onClose();
    setName("");
    setIdentifier("");
    setDescription("");
    setEmoji(null);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <div className="w-full max-w-md rounded-lg border border-custom-border-200 bg-custom-background-100 p-6 shadow-xl">
        {/* Header */}
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-lg font-semibold text-custom-text-100">
            创建项目
          </h2>
          <button
            onClick={onClose}
            className="rounded-md p-1 text-custom-text-300 hover:bg-custom-background-80 transition-colors"
          >
            <X className="size-4" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          {/* Emoji logo */}
          <div className="flex justify-center">
            <EmojiPicker
              value={emoji}
              onChange={setEmoji}
              label={
                <span className="text-3xl">
                  {emoji
                    ? String.fromCodePoint(parseInt(emoji, 10))
                    : "📁"}
                </span>
              }
            />
          </div>

          {/* Name */}
          <div className="flex flex-col gap-1">
            <label className="text-xs font-medium text-custom-text-200">
              项目名称
            </label>
            <input
              type="text"
              value={name}
              onChange={(e) => {
                setName(e.target.value);
                if (!identifier)
                  setIdentifier(
                    e.target.value
                      .split(" ")
                      .map((w) => w.charAt(0))
                      .join("")
                      .toUpperCase()
                      .slice(0, 5),
                  );
              }}
              placeholder="项目名称"
              maxLength={100}
              required
              className="rounded-md border border-custom-border-200 bg-custom-background-90 px-3 py-2 text-sm text-custom-text-100 outline-none focus:border-custom-primary"
            />
          </div>

          {/* Identifier */}
          <div className="flex flex-col gap-1">
            <label className="text-xs font-medium text-custom-text-200">
              标识符
            </label>
            <input
              type="text"
              value={identifier}
              onChange={(e) =>
                setIdentifier(
                  e.target.value.toUpperCase().replace(/[^A-Z0-9]/g, "").slice(0, 10),
                )
              }
              placeholder="FF"
              maxLength={10}
              required
              className="rounded-md border border-custom-border-200 bg-custom-background-90 px-3 py-2 text-sm text-custom-text-100 outline-none focus:border-custom-primary"
            />
          </div>

          {/* Description */}
          <div className="flex flex-col gap-1">
            <label className="text-xs font-medium text-custom-text-200">
              描述（可选）
            </label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              rows={2}
              className="rounded-md border border-custom-border-200 bg-custom-background-90 px-3 py-2 text-sm text-custom-text-100 outline-none focus:border-custom-primary resize-none"
            />
          </div>

          {/* Actions */}
          <div className="flex justify-end gap-2 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="rounded-md border border-custom-border-200 px-3 py-1.5 text-sm text-custom-text-200 hover:bg-custom-background-80 transition-colors"
            >
              取消
            </button>
            <button
              type="submit"
              disabled={!name || !identifier}
              className="rounded-md bg-custom-primary px-3 py-1.5 text-sm font-medium text-white transition-colors hover:bg-custom-primary-200 disabled:opacity-50"
            >
              创建
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
