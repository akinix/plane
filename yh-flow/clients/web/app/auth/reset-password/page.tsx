// FLOW: Reset-password page — set new password with token (AUTH-04, D-10)
import { useState } from "react";
import { useNavigate, useSearchParams, Link } from "react-router";
import { useStore } from "@/lib/store-context";

export default function ResetPasswordPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const token = searchParams.get("token");
  const { auth } = useStore();
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    if (!token) {
      setError("无效的重置链接：缺少 token");
      return;
    }
    if (password !== confirmPassword) {
      setError("两次输入的密码不一致");
      return;
    }
    if (password.length < 6) {
      setError("密码长度至少为6个字符");
      return;
    }

    setIsSubmitting(true);
    try {
      await auth.resetPassword(token, password);
      navigate("/auth/sign-in?reset=success", { replace: true });
    } catch (err: any) {
      setError(err?.error || err?.message || "重置失败，链接可能已过期");
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!token) {
    return (
      <div className="flex h-screen w-full items-center justify-center bg-custom-background-100">
        <div className="mx-auto w-full max-w-md rounded-lg bg-custom-background-90 p-8 text-center shadow-lg">
          <div className="mb-4 rounded-md bg-red-500/10 px-4 py-3 text-sm text-red-500">
            无效的重置链接：缺少验证参数
          </div>
          <Link to="/auth/forgot-password" className="text-sm text-custom-primary-100 hover:underline">
            重新发送重置链接
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="flex h-screen w-full items-center justify-center bg-custom-background-100">
      <div className="mx-auto w-full max-w-md rounded-lg bg-custom-background-90 p-8 shadow-lg">
        <div className="mb-8 text-center">
          <h1 className="text-2xl font-semibold text-custom-text-100">设置新密码</h1>
          <p className="mt-1 text-sm text-custom-text-300">请输入您的新密码</p>
        </div>

        {error && (
          <div className="mb-4 rounded-md bg-red-500/10 px-4 py-3 text-sm text-red-500">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label htmlFor="password" className="block text-sm font-medium text-custom-text-200">
              新密码
            </label>
            <div className="relative mt-1">
              <input
                id="password"
                type={showPassword ? "text" : "password"}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="至少6个字符"
                required
                minLength={6}
                className="block w-full rounded-md border border-custom-border-200 bg-custom-background-80 px-3 py-2 pr-10 text-sm text-custom-text-100 placeholder-custom-text-400 focus:border-custom-primary-100 focus:outline-none"
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-custom-text-300 hover:text-custom-text-200"
                tabIndex={-1}
              >
                {showPassword ? "🙈" : "👁"}
              </button>
            </div>
          </div>

          <div>
            <label htmlFor="confirmPassword" className="block text-sm font-medium text-custom-text-200">
              确认新密码
            </label>
            <input
              id="confirmPassword"
              type="password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              placeholder="再次输入新密码"
              required
              className="mt-1 block w-full rounded-md border border-custom-border-200 bg-custom-background-80 px-3 py-2 text-sm text-custom-text-100 placeholder-custom-text-400 focus:border-custom-primary-100 focus:outline-none"
            />
          </div>

          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full rounded-md bg-custom-primary-100 px-4 py-2 text-sm font-medium text-white hover:bg-custom-primary-200 disabled:opacity-50"
          >
            {isSubmitting ? (
              <span className="flex items-center justify-center gap-2">
                <span className="inline-block size-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
                重置中...
              </span>
            ) : (
              "重置密码"
            )}
          </button>
        </form>

        <p className="mt-6 text-center text-sm text-custom-text-300">
          <Link to="/auth/sign-in" className="text-custom-primary-100 hover:underline">
            返回登录
          </Link>
        </p>
      </div>
    </div>
  );
}
