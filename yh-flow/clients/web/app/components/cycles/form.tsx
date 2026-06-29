// FLOW: Forked from Plane cycles/form.tsx
// FLOW: CycleForm — Cycle 创建/编辑表单
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import type { ICycle } from "@plane/types";
import { useCycleMutations } from "@/lib/hooks/use-cycles";

type Props = {
  onClose?: () => void;
  workspaceId: string;
  projectId: string;
  data?: ICycle | null;
};

const defaultValues: Partial<ICycle> = {
  name: "",
  description: "",
  start_date: null,
  end_date: null,
};

// Default dates: start=today, end=today+30
const todayStr = () => new Date().toISOString().split("T")[0];
const futureStr = () => {
  const d = new Date();
  d.setDate(d.getDate() + 30);
  return d.toISOString().split("T")[0];
};

export function CycleForm(props: Props) {
  const { onClose, workspaceId, projectId, data } = props;
  const isEdit = !!data;
  const { createCycle, updateCycle } = useCycleMutations();

  const {
    register,
    handleSubmit,
    reset,
    watch,
    formState: { errors },
  } = useForm<Partial<ICycle>>({
    defaultValues: {
      name: data?.name || "",
      description: data?.description || "",
      start_date: data?.start_date || todayStr(),
      end_date: data?.end_date || futureStr(),
    },
  });

  const startDate = watch("start_date");
  const endDate = watch("end_date");

  useEffect(() => {
    if (data) {
      reset({
        name: data.name || "",
        description: data.description || "",
        start_date: data.start_date ?? todayStr(),
        end_date: data.end_date ?? futureStr(),
      });
    }
  }, [data, reset]);

  // Validate end_date >= start_date
  const isDateValid = () => {
    if (!startDate || !endDate) return true;
    return new Date(endDate) >= new Date(startDate);
  };

  const onSubmit = async (formData: Partial<ICycle>) => {
    const payload = {
      ...formData,
      project_id: projectId,
      workspace_id: workspaceId,
    };

    if (isEdit && data?.id) {
      updateCycle.mutate({ cycleId: data.id, data: payload });
    } else {
      createCycle.mutate(payload);
    }
    onClose?.();
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5 p-5">
      <div className="space-y-3">
        <div className="space-y-1">
          <input
            {...register("name", {
              required: "名称为必填项",
              maxLength: { value: 255, message: "名称不能超过255个字符" },
            })}
            type="text"
            placeholder="周期名称"
            className="w-full rounded-md border border-subtle bg-surface-1 px-3 py-2 text-14 text-primary outline-none placeholder:text-placeholder focus:border-accent-primary"
            autoFocus
          />
          {errors.name && <span className="text-11 text-danger-primary">{errors.name.message}</span>}
        </div>
        <div>
          <textarea
            {...register("description")}
            placeholder="周期描述（选填）"
            rows={3}
            className="min-h-24 w-full resize-none rounded-md border border-subtle bg-surface-1 px-3 py-2 text-14 text-primary outline-none placeholder:text-placeholder focus:border-accent-primary"
          />
        </div>
        <div className="flex flex-wrap items-center gap-3">
          <div className="flex flex-col gap-1">
            <label className="text-12 text-tertiary">开始日期</label>
            <input
              type="date"
              {...register("start_date")}
              className="rounded-md border border-subtle bg-surface-1 px-3 py-1.5 text-13 text-primary outline-none"
            />
          </div>
          <span className="mt-6 text-tertiary">→</span>
          <div className="flex flex-col gap-1">
            <label className="text-12 text-tertiary">结束日期</label>
            <input
              type="date"
              {...register("end_date", {
                validate: () => isDateValid() || "结束日期不能早于开始日期",
              })}
              className="rounded-md border border-subtle bg-surface-1 px-3 py-1.5 text-13 text-primary outline-none"
            />
          </div>
        </div>
        {errors.end_date && <span className="text-11 text-danger-primary">{errors.end_date.message}</span>}
        {!isDateValid() && (
          <span className="text-11 text-danger-primary">结束日期不能早于开始日期</span>
        )}
      </div>
      <div className="flex items-center justify-end gap-2 border-t border-subtle pt-4">
        <button
          type="button"
          className="rounded-md bg-surface-2 px-4 py-2 text-13 font-medium text-primary hover:bg-surface-3"
          onClick={onClose}
        >
          取消
        </button>
        <button
          type="submit"
          disabled={!isDateValid()}
          className="rounded-md bg-accent-primary px-4 py-2 text-13 font-medium text-white hover:bg-accent-primary/90 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {isEdit ? "更新周期" : "创建周期"}
        </button>
      </div>
    </form>
  );
}
