// FLOW: NotificationSkeleton — Loading skeleton for notification list (per UI-SPEC)
import { Loader } from "@/lib/ui/loader";

export const NotificationSkeleton = () => {
  return (
    <div className="flex flex-col gap-0.5 px-4 py-3">
      {Array.from({ length: 6 }).map((_, i) => (
        <div key={i} className="flex items-start gap-3 py-3">
          {/* Avatar placeholder */}
          <Loader className="flex-shrink-0">
            <Loader.Item className="size-8 rounded-full" />
          </Loader>
          {/* Text lines */}
          <div className="min-w-0 flex-1 space-y-2">
            <Loader>
              <Loader.Item className="h-3 w-3/4 rounded" />
            </Loader>
            <Loader>
              <Loader.Item className="h-3 w-1/2 rounded" />
            </Loader>
          </div>
        </div>
      ))}
    </div>
  );
};
