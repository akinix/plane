---
phase: 14-auth
plan: 04
wave: 3
type: summary
status: delivered
requirements: [AUTH-01, AUTH-02, AUTH-03, AUTH-04, AUTH-05, AUTH-06]
---

# Phase 14, Plan 04 — 认证页面 Summary

## Objective

实现完整的 JWT 认证流程：登录、注册、忘记密码/重置密码、Token 持久化、退出登录、路由保护。

## Changes Made

### New Files

| File                                          | Purpose                                                                                       |
| --------------------------------------------- | --------------------------------------------------------------------------------------------- |
| `app/store/auth.store.ts`                     | AuthStore MobX 状态管理（signIn/signUp/signOut/sendResetPasswordLink/resetPassword/initAuth） |
| `app/store/root.store.ts`                     | CoreRootStore 根 Store，实例化 AuthStore                                                      |
| `app/lib/store-context.tsx`                   | StoreProvider React Context + useStore hook                                                   |
| `app/lib/wrappers/authentication-wrapper.tsx` | AuthenticationWrapper 路由保护组件（3 种页面类型：PUBLIC/NON_AUTHENTICATED/AUTHENTICATED）    |
| `app/auth/sign-in/page.tsx`                   | 登录页面（邮箱+密码，next_path 重定向）                                                       |
| `app/auth/sign-up/page.tsx`                   | 注册页面（邮箱+密码+确认密码，注册后自动登录 D-09）                                           |
| `app/auth/forgot-password/page.tsx`           | 忘记密码页面（发送重置链接）                                                                  |
| `app/auth/reset-password/page.tsx`            | 重置密码页面（从 URL query 读取 token）                                                       |
| `app/page.tsx`                                | 首页占位页（带 Sign out 按钮，AUTH-05）                                                       |
| `src/lib/types/error.ts`                      | 标准错误响应类型                                                                              |
| `src/lib/services/auth.service.ts`            | AuthService（JWT 认证服务，10 个端点方法）                                                    |
| `src/lib/services/flow-api.service.ts`        | FlowApiService 基类（JWT Bearer + humps + 错误标准化）                                        |
| `src/lib/services/index.ts`                   | Services barrel export                                                                        |

### Modified Files

| File                    | Change                                                                                        |
| ----------------------- | --------------------------------------------------------------------------------------------- |
| `app/routes.ts`         | 注册 5 个路由（/auth/sign-in, /auth/sign-up, /auth/forgot-password, /auth/reset-password, /） |
| `app/provider.tsx`      | 添加 StoreProvider 和 AuthInitializer                                                         |
| `app/root.tsx`          | 无修改                                                                                        |
| `src/lib/types/auth.ts` | ILoginTokenResponse 字段改为 camelCase（匹配 humps 运行时转换）                               |
| `vite.config.ts`        | 添加 /api proxy 配置                                                                          |
| `package.json`          | 添加 @types/humps 依赖                                                                        |
| `.env`                  | 环境变量配置文件                                                                              |

## Dev Notes

- **Path fix**: 14-03 worktree agent 将服务文件创建在 `clients/web/`（仓库根目录）而非 `yh-flow/clients/web/`，已修复
- **Type fix**: `ILoginTokenResponse` 的 `access_token`/`refresh_token` 改为 `accessToken`/`refreshToken`，匹配 humps camelizeKeys 运行时转换
- **CamelCase convention**: 所有响应类型字段使用 camelCase，因为 humps 在 Axios 响应拦截器中自动将 .NET 的 snake_case 转换为 camelCase
- **Import convention**: 由于 `@/*` tsconfig 路径映射到 `app/*`，`src/lib/` 下的文件使用相对路径导入（`../../src/lib/services/auth.service`）

## Verification

- `npx tsc --noEmit` — auth 相关代码无错误（仅有预存的 fork 问题：root.tsx ReactNode 类型和 editor 缺失模块）
- Task 1 ✓: AuthStore + RootStore + StoreProvider
- Task 2 ✓: 4 个认证页面 + 路由配置
- Task 3 ✓: AuthenticationWrapper 路由保护 + 首页退出按钮

## Next Steps

- 继续 Phase 15~20 的 autonomous 执行
- **Known improvement**: 当前 AuthStore.initAuth 在有 token 时将当前用户设为 `{ id: "authenticated" }` 占位，后续应调用 `/api/v1/users/me/` 获取真实用户信息
