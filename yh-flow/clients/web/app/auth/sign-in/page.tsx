// FLOW: Sign-in page — email + password authentication (AUTH-01, AUTH-02)
import { useState } from "react";
import { useNavigate, useSearchParams, Link } from "react-router";
import { useStore } from "@/lib/store-context";

export default function SignInPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { auth } = useStore();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setIsSubmitting(true);
    try {
      await auth.signIn(email, password);
      const nextPath = searchParams.get("next_path");
      navigate(nextPath ? decodeURIComponent(nextPath) : "/", { replace: true });
    } catch (err: any) {
      setError(err?.error || err?.message || "登录失败，请检查邮箱和密码");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="flex h-screen w-full items-center justify-center bg-custom-background-100">
      <div className="mx-auto w-full max-w-md rounded-lg bg-custom-background-90 p-8 shadow-lg">
        {/* Logo area */}
        <div className="mb-8 text-center">
          <h1 className="text-2xl font-semibold text-custom-text-100">Flow</h1>
          <p className="mt-1 text-sm text-custom-text-300">登录到您的账户</p>
        </div>

        {error && (
          <div className="mb-4 rounded-md bg-red-500/10 px-4 py-3 text-sm text-red-500">
            {error}
          </div>
        )}

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

          <div>
            <label htmlFor="password" className="block text-sm font-medium text-custom-text-200">
              密码
            </label>
            <div className="relative mt-1">
              <input
                id="password"
                type={showPassword ? "text" : "password"}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="输入密码"
                required
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

          <div className="flex items-center justify-end">
            <Link
              to="/auth/forgot-password"
              className="text-sm text-custom-primary-100 hover:underline"
            >
              忘记密码？
            </Link>
          </div>

          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full rounded-md bg-custom-primary-100 px-4 py-2 text-sm font-medium text-white hover:bg-custom-primary-200 disabled:opacity-50"
          >
            {isSubmitting ? (
              <span className="flex items-center justify-center gap-2">
                <span className="inline-block size-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
                登录中...
              </span>
            ) : (
              "登录"
            )}
          </button>
        </form>

        <p className="mt-6 text-center text-sm text-custom-text-300">
          还没有账户？{" "}
          <Link to="/auth/sign-up" className="text-custom-primary-100 hover:underline">
            注册
          </Link>
        </p>
      </div>
    </div>
  );
}
