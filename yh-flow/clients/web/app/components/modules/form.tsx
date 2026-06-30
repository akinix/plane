// FLOW: Forked from Plane modules/form.tsx
// FLOW: ModuleForm — Module 创建/编辑表单
import { useEffect } from "react";
import { Controller, useForm } from "react-hook-form";
import type { IModule } from "@plane/types";
import { Input, TextArea } from "@plane/ui";
import { getDate, renderFormattedPayloadDate } from "@plane/utils";
import { useModuleMutations } from "@/../src/lib/hooks/use-modules";
import { ModuleStatusDropdown } from "./module-status-dropdown";

type Props = {
  onClose?: () => void;
  workspaceId: string;
  projectId: string;
  data?: Partial<IModule> | null;
};

const STATUS_OPTIONS = [
  { value: "backlog", label: "待开始" },
  { value: "in_progress", label: "进行中" },
  { value: "completed", label: "已完成" },
  { value: "cancelled", label: "已取消" },
];

export function ModuleForm(props: Props) {
  const { onClose, workspaceId, projectId, data } = props;
  const { createModule, updateModule } = useModuleMutations();
  const isEdit = !!data?.id;

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<Partial<IModule>>({
    defaultValues: {
      workspace_id: workspaceId,
      project_id: projectId,
      name: data?.name ?? "",
      description: data?.description ?? "",
      status: data?.status ?? "backlog",
      lead_id: data?.lead_id ?? null,
      member_ids: data?.member_ids ?? [],
      start_date: data?.start_date ?? null,
      target_date: data?.target_date ?? null,
    },
  });

  useEffect(() => {
    if (data) {
      reset({
        workspace_id: workspaceId,
        project_id: projectId,
        name: data.name ?? "",
        description: data.description ?? "",
        status: data.status ?? "backlog",
        lead_id: data.lead_id ?? null,
        member_ids: data.member_ids ?? [],
        start_date: data.start_date ?? null,
        target_date: data.target_date ?? null,
      });
    }
  }, [data, reset, workspaceId, projectId]);

  const onSubmit = async (formData: Partial<IModule>) => {
    const payload: Partial<IModule> = {
      ...formData,
    };

    if (isEdit && data?.id) {
      await updateModule.mutateAsync({ moduleId: data.id, data: payload });
    } else {
      await createModule.mutateAsync(payload);
    }

    onClose?.();
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <div className="space-y-5 p-5">
        <h3 className="text-18 font-medium text-secondary">
          {isEdit ? "编辑模块" : "创建模块"}
        </h3>
        <div className="space-y-3">
          {/* 名称 */}
          <div className="space-y-1">
            <input
              {...register("name", {
                required: "名称为必填项",
                maxLength: { value: 255, message: "名称不能超过 255 个字符" },
              })}
              type="text"
              placeholder="模块名称"
              className="w-full rounded-md border border-subtle bg-surface-1 px-3 py-2 text-14 text-primary placeholder:text-tertiary focus:outline-none focus:border-accent-primary"
              autoFocus
            />
            {errors.name && (
              <span className="text-11 text-danger-primary">{errors.name.message as string}</span>
            )}
          </div>

          {/* 描述 */}
          <div>
            <textarea
              {...register("description")}
              placeholder="模块描述（可选）"
              rows={4}
              className="min-h-24 w-full resize-none rounded-md border border-subtle bg-surface-1 px-3 py-2 text-14 text-primary placeholder:text-tertiary focus:outline-none focus:border-accent-primary"
            />
          </div>

          {/* 日期 + 状态 */}
          <div className="flex flex-wrap items-center gap-2">
            <Controller
              control={control}
              name="start_date"
              render={({ field: { value, onChange } }: { field: { value: any; onChange: (...args: any[]) => void } }) => (
                <input
                  type="date"
                  value={value ? new Date(value).toISOString().split("T")[0] : ""}
                  onChange={(e) => onChange(e.target.value ? new Date(e.target.value).toISOString() : null)}
                  className="h-7 rounded-md border border-subtle bg-surface-1 px-2 text-12 text-primary focus:outline-none focus:border-accent-primary"
                  placeholder="开始日期"
                />
              )}
            />
            <span className="text-tertiary">至</span>
            <Controller
              control={control}
              name="target_date"
              render={({ field: { value, onChange } }: { field: { value: any; onChange: (...args: any[]) => void } }) => (
                <input
                  type="date"
                  value={value ? new Date(value).toISOString().split("T")[0] : ""}
                  onChange={(e) => onChange(e.target.value ? new Date(e.target.value).toISOString() : null)}
                  className="h-7 rounded-md border border-subtle bg-surface-1 px-2 text-12 text-primary focus:outline-none focus:border-accent-primary"
                  placeholder="目标日期"
                />
              )}
            />
            <Controller
              control={control}
              name="status"
              render={({ field: { value, onChange } }: { field: { value: any; onChange: (...args: any[]) => void } }) => (
                <select
                  value={value ?? "backlog"}
                  onChange={(e) => onChange(e.target.value)}
                  className="h-7 rounded-md border border-subtle bg-surface-1 px-2 text-12 text-primary focus:outline-none focus:border-accent-primary"
                >
                  {STATUS_OPTIONS.map((opt) => (
                    <option key={opt.value} value={opt.value}>{opt.label}</option>
                  ))}
                </select>
              )}
            />
          </div>
        </div>
      </div>

      {/* 按钮 */}
      <div className="flex items-center justify-end gap-2 border-t border-subtle px-5 py-4">
        <button
          type="button"
          onClick={onClose}
          className="rounded-md border border-subtle px-4 py-2 text-13 text-primary hover:bg-surface-1"
        >
          取消
        </button>
        <button
          type="submit"
          disabled={isSubmitting}
          className="rounded-md bg-accent-primary px-4 py-2 text-13 text-white hover:bg-accent-primary/90 disabled:opacity-50"
        >
          {isSubmitting ? "保存中..." : isEdit ? "更新模块" : "创建模块"}
        </button>
      </div>
    </form>
  );
}
