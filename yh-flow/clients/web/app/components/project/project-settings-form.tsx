// FLOW: ProjectSettingsForm — edit project name, identifier, description, emoji logo (PROJ-03)
import { useState, type FormEvent } from "react";
import { Save } from "lucide-react";
import { useProject } from "../../../src/lib/hooks/use-projects";
import { EmojiPicker } from "../../../src/lib/ui/emoji-picker";
import { cn } from "@plane/utils";

const PLACEHOLDER_EMOJI = "128196";

type TProps = { workspaceId: string; projectId: string };

export const ProjectSettingsForm = ({
  workspaceId,
  projectId,
}: TProps) => {
  const { data: project, isLoading } = useProject(workspaceId, projectId);
  const [name, setName] = useState("");
  const [ident, setIdent] = useState("");
  const [description, setDescription] = useState("");
  const [emoji, setEmoji] = useState<string | null>(null);
  const [saved, setSaved] = useState(false);

  // Sync on load
  if (project && name === "") {
    setName(project.name);
    setIdent(project.identifier);
    setDescription(project.description ?? "");
    setEmoji(project.logo_props?.emoji?.value ?? null);
  }

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    console.log("Project settings saved:", {
      workspaceId,
      projectId,
      name,
      identifier: ident,
      description,
      emoji,
    });
    setSaved(true);
    setTimeout(() => setSaved(false), 2000);
  };

  if (isLoading) {
    return (
      <div className="h-32 animate-pulse rounded-lg bg-custom-background-80" />
    );
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-6">
      {/* Logo */}
      <div className="flex items-center gap-4">
        <EmojiPicker
          value={emoji}
          onChange={setEmoji}
          label={
            <span className="text-2xl">
              {emoji
                ? String.fromCodePoint(parseInt(emoji, 10))
                : String.fromCodePoint(parseInt(PLACEHOLDER_EMOJI, 10))}
            </span>
          }
        />
        <div>
          <p className="text-sm font-medium text-custom-text-100">项目图标</p>
          <p className="text-xs text-custom-text-300">点击选择 emoji</p>
        </div>
      </div>

      {/* Name */}
      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium text-custom-text-200">名称</label>
        <input
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          maxLength={100}
          required
          className="rounded-md border border-custom-border-200 bg-custom-background-100 px-3 py-2 text-sm text-custom-text-100 outline-none focus:border-custom-primary"
        />
      </div>

      {/* Identifier */}
      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium text-custom-text-200">
          标识符
        </label>
        <input
          type="text"
          value={ident}
          onChange={(e) =>
            setIdent(
              e.target.value.toUpperCase().replace(/[^A-Z0-9]/g, "").slice(0, 10),
            )
          }
          maxLength={10}
          required
          className="rounded-md border border-custom-border-200 bg-custom-background-100 px-3 py-2 text-sm text-custom-text-100 outline-none focus:border-custom-primary"
        />
      </div>

      {/* Description */}
      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium text-custom-text-200">
          描述
        </label>
        <textarea
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          rows={3}
          className="rounded-md border border-custom-border-200 bg-custom-background-100 px-3 py-2 text-sm text-custom-text-100 outline-none focus:border-custom-primary resize-none"
        />
      </div>

      {/* Submit */}
      <div>
        <button
          type="submit"
          className={cn(
            "flex items-center gap-2 rounded-md px-4 py-2 text-sm font-medium transition-colors",
            saved
              ? "bg-green-500 text-white"
              : "bg-custom-primary text-white hover:bg-custom-primary-200",
          )}
        >
          <Save className="size-4" />
          <span>{saved ? "已保存" : "保存"}</span>
        </button>
      </div>
    </form>
  );
};
