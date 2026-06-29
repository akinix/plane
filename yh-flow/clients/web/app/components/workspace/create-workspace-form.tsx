// FLOW: CreateWorkspaceForm — standalone workspace creation form (WORK-01)
import { useState, type FormEvent } from "react";
import { useNavigate } from "react-router";
import { EmojiPicker } from "../../../src/lib/ui/emoji-picker";

const slugify = (text: string) =>
  text
    .toLowerCase()
    .replace(/[^a-z0-9一-鿿]+/g, "-")
    .replace(/^-|-$/g, "");

export const CreateWorkspaceForm = () => {
  const navigate = useNavigate();
  const [name, setName] = useState("");
  const [slug, setSlug] = useState("");
  const [description, setDescription] = useState("");
  const [emoji, setEmoji] = useState<string | null>(null);
  const [autoSlug, setAutoSlug] = useState(true);

  const handleNameChange = (val: string) => {
    setName(val);
    if (autoSlug) setSlug(slugify(val));
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    const newWorkspace = { name, slug, description, emoji };
    console.log("Workspace created:", newWorkspace);
    // Mock: navigate to the new workspace
    navigate(`/workspaces/${slug}`, { replace: true });
  };

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-5">
      {/* Emoji logo */}
      <div className="flex justify-center">
        <EmojiPicker
          value={emoji}
          onChange={setEmoji}
          label={
            <span className="text-4xl">
              {emoji ? String.fromCodePoint(parseInt(emoji, 10)) : "😀"}
            </span>
          }
        />
      </div>

      {/* Name */}
      <div className="flex flex-col gap-1.5">
        <label className="text-sm font-medium text-custom-text-200">名称</label>
        <input
          type="text"
          value={name}
          onChange={(e) => handleNameChange(e.target.value)}
          placeholder="工作区名称"
          maxLength={100}
          required
          className="rounded-md border border-custom-border-200 bg-custom-background-100 px-3 py-2 text-sm text-custom-text-100 outline-none focus:border-custom-primary"
        />
      </div>

      {/* Slug */}
      <div className="flex flex-col gap-1.5">
        <label className="text-sm font-medium text-custom-text-200">
          标识符
        </label>
        <input
          type="text"
          value={slug}
          onChange={(e) => {
            setAutoSlug(false);
            setSlug(slugify(e.target.value));
          }}
          placeholder="workspace-slug"
          required
          className="rounded-md border border-custom-border-200 bg-custom-background-100 px-3 py-2 text-sm text-custom-text-100 outline-none focus:border-custom-primary"
        />
      </div>

      {/* Description */}
      <div className="flex flex-col gap-1.5">
        <label className="text-sm font-medium text-custom-text-200">
          描述（可选）
        </label>
        <textarea
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="工作区描述"
          rows={3}
          className="rounded-md border border-custom-border-200 bg-custom-background-100 px-3 py-2 text-sm text-custom-text-100 outline-none focus:border-custom-primary resize-none"
        />
      </div>

      {/* Submit */}
      <button
        type="submit"
        disabled={!name || !slug}
        className="rounded-md bg-custom-primary px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-custom-primary-200 disabled:opacity-50 disabled:cursor-not-allowed"
      >
        创建工作区
      </button>
    </form>
  );
};
