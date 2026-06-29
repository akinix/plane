// FLOW: Forgot-password page — send reset link (AUTH-04, D-10)
import { useState } from "react";
import { Link } from "react-router";
import { useStore } from "@/lib/store-context";

export default function ForgotPasswordPage() {
  const { auth } = useStore();
  const [email, setEmail] = useState("");
  const [isSuccess, setIsSuccess] = useState(false);
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setIsSubmitting(true);
    try {
      await auth.sendResetPasswordLink(email);
      setIsSuccess(true);
    } catch (err: any) {
      setError(err?.error || err?.message || "发送失败，请重试");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="flex h-screen w-full items-center justify-center bg-custom-background-100">
      <div className="mx-auto w-full max-w-md rounded-lg bg-custom-background-90 p-8 shadow-lg">
        <div className="mb-8 text-center">
          <h1 className="text-2xl font-semibold text-custom-text-100">重置密码</h1>
          <p className="mt-1 text-sm text-custom-text-300">
            输入您的邮箱，我们将发送重置链接
          </p>
        </div>

        {error && (
          <div className="mb-4 rounded-md bg-red-500/10 px-4 py-3 text-sm text-red-500">
            {error}
          </div>
        )}

        {isSuccess ? (
          <div className="text-center">
            <div className="mb-4 rounded-md bg-green-500/10 px-4 py-3 text-sm text-green-500">
              重置链接已发送到您的邮箱，请查收
            </div>
            <Link
              to="/auth/sign-in"
              className="text-sm text-custom-primary-100 hover:underline"
            >
              返回登录
            </Link>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label htmlFor="email" className="block text-sm font-medium text-custom-text-200">
                邮箱
              </label>
              <input
                id="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="name@example.com"
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
                  发送中...
                </span>
              ) : (
                "发送重置链接"
              )}
            </button>

            <p className="text-center text-sm text-custom-text-300">
              <Link to="/auth/sign-in" className="text-custom-primary-100 hover:underline">
                返回登录
              </Link>
            </p>
          </form>
        )}
      </div>
    </div>
  );
}
