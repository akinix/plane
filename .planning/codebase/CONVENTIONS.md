# Coding Conventions

**Analysis Date:** 2026-06-16

## Naming Patterns

**Files:**
- React components: PascalCase (`button.tsx`, `not-authorized-view.tsx`)，实际以 kebab-case 为主
- Utility/helper files: kebab-case (`classname.tsx`, `get-icon-for-link.ts`，`use-translation.ts`)
- Store files: kebab-case with `.store.ts` suffix (`dashboard.store.ts`，`filter.store.ts`，`user.store.ts`，`workspace.store.ts`)
- Service files: kebab-case with `.service.ts` suffix (`auth.service.ts`，`workspace.service.ts`，`indexedDB.service.ts`)
- Test files: `*.test.ts` (frontend), `test_*.py` (backend)
- Storybook stories: `*.stories.tsx`

**Directories:**
- kebab-case for all directories (`shared-state/`，`form-fields/`，`auth-forms/`，`base-layouts/`)
- Group folders in parentheses for route grouping: `(all)/`, `(projects)/`, `(detail)/`, `(list)/`, `(settings)/`

**Functions:**
- camelCase for functions and methods (`fetchHomeDashboardWidgets`, `getWidgetDetails`, `handleSubmit`, `coerceToString`)
- Arrow function properties for computed getters in stores: `getWidgetDetails = computedFn(...)`
- async functions use `async/await` consistently

**Variables:**
- camelCase for variables (`workspaceSlug`, `homeDashboardId`, `widgetStats`, `dashboardService`, `currentWorkspace`)
- Private class fields prefixed internally (e.g., `_rootStore`), not enforced by convention
- Unused variables prefixed with `_`

**Types/Interfaces:**
- PascalCase for types and interfaces (`TDashboardProps`, `TWidgetKeys`，`IWorkspaceStore`，`ButtonProps`)
- Type aliases preferred over interfaces for prop definitions (e.g., `type Props = { ... }` in React components)
- Interfaces used for Store contracts (`IWorkspaceStore`, `IDashboardStore`)
- Type prefix `T` for complex type aliases (`TButtonVariant`, `TButtonSizes`，`TAvatarSize`，`TWidgetKeys`)
- Interface prefix `I` for store interfaces (`IUserStore`，`IWorkspaceStore`，`IDashboardStore`)
- Discriminated union types via `@plane/types` package

**Enums/Constants:**
- SCREAMING_SNAKE_CASE for constants and enum-like structures (`LOGICAL_OPERATOR`, `FALLBACK_LANGUAGE`，`LANGUAGE_STORAGE_KEY`，`NAMESPACES`，`DEFAULT_NAMESPACE`)

## Code Style

**Formatting (oxfmt via `.oxfmtrc.json`):**
- Print width: 120 characters
- Tab width: 2 spaces
- Trailing commas: `es5` (objects, arrays, but not function params)
- Tailwind CSS class sorting: enabled, functions include `cn`, `clsx`, `cva`
- Codemods package: 80 character print width override

**Linting (OxLint via `.oxlintrc.json`):**
- Plugins: `react`, `typescript`, `jsx-a11y`, `import`, `promise`, `unicorn`, `oxc`
- Categories: correctness (warn), suspicious (warn), perf (warn)
- React: `react-in-jsx-scope: off` (React 17+ JSX transform), `prop-types: off` (TypeScript types used instead)
- Unicorn: `filename-case: off` (kebab-case filenames allowed), `no-null: off`, `prevent-abbreviations: off`
- `no-unused-vars`: warn, ignores `_` prefixed vars/args/errors
- Ignored paths: `.cache/**`, `.next/**`, `.turbo/**`, `.vite/**`, `dist/**`, `build/**`, `coverage/**`, `storybook-static/**`

**Pre-commit (Husky + lint-staged):**
- `lint-staged` config in root `package.json`
- `*.{js,jsx,ts,tsx,cjs,mjs,cts,mts,json,css,md}`: run `oxfmt --no-error-on-unmatched-pattern`
- `*.{js,jsx,ts,tsx,cjs,mjs,cts,mts}`: run `oxlint --fix --deny-warnings`

## TypeScript Configuration

**Base config (`packages/typescript-config/base.json`):**
- `strict: true` (all strict checks enabled)
- `exactOptionalPropertyTypes: true` (overridden to `false` in most consuming packages)
- `noUnusedLocals: true` (overridden in many packages), `noUnusedParameters: true`
- `noImplicitReturns: true`, `noImplicitOverride: true`
- `verbatimModuleSyntax: true` (forces `import type` for type-only imports)
- `isolatedModules: true`, `module: preserve`, `moduleResolution: bundler`
- `target: esnext`, `lib: ["es2023"]`
- `incremental: true` (with tsBuildInfoFile: `.turbo/tsconfig.tsbuildinfo`)

**Overrides in consumer packages:**
- `exactOptionalPropertyTypes`, `noUnusedLocals`, `noUnusedParameters`, `noImplicitReturns` often set to `false`
- Package-specific path aliases: `@/*` → `./src/*` or `./core/*`
- React packages add `jsx: react-jsx` and DOM libs

## React Component Patterns

**Component declaration:**
```tsx
// Functional component with forwardRef (when ref forwarding needed)
const Button = React.forwardRef(function Button(props: ButtonProps, ref: React.ForwardedRef<HTMLButtonElement>) {
  // ...
});
Button.displayName = "plane-ui-button";

// Simple functional component
export function Avatar(props: Props) {
  const { name, size = "md", ... } = props;
  return ( /* JSX */ );
}

// Component with observer
export const NotAuthorizedView = observer(function NotAuthorizedView(props: Props) {
  // ...
});
```

**Props typing pattern:**
```tsx
// Type alias for props (not interface)
type Props = {
  /** JSDoc description */
  name?: string;
  /** JSDoc with @default */
  size?: TAvatarSize; // @default "md"
  className?: string;
};

// Interface for props when extending HTML attributes
export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: TButtonVariant;
  size?: TButtonSizes;
  children: React.ReactNode;
}
```

**Default props:** Destructured directly in function parameters with default values (`size = "md"`，`showTooltip = true`).

**Component export patterns:**
- Named exports preferred: `export function Avatar(props: Props)`, `export { Button }`
- Default exports for page-level components wrapped in `observer`: `export default observer(WorkspaceDashboardPage)`
- Barrel exports from `index.ts` files: `export * from "./avatar"`

**File structure per component:**
```
src/
  component-name/
    component-name.tsx      # Main component
    component-name.stories.tsx  # Storybook stories (if applicable)
    helper.tsx              # Variants, styling helpers, constants
    types.ts                # Types (when complex)
```

## MobX State Management Patterns

**Store class structure (`apps/web/core/store/*.store.ts`):**
```typescript
import { action, computed, makeObservable, observable, runInAction } from "mobx";
import { computedFn } from "mobx-utils";

export interface IExampleStore {
  data: Record<string, any>;
  getDetails: (id: string) => any;
  fetchData: (id: string) => Promise<any>;
}

export class ExampleStore implements IExampleStore {
  // Observables
  data: Record<string, any> = {};
  isLoading: boolean = false;
  error: any = null;
  // Injected dependencies
  routerStore;
  exampleService;

  constructor(_rootStore: CoreRootStore) {
    makeObservable(this, {
      data: observable,
      isLoading: observable.ref,
      error: observable.ref,
      // Computed
      processedData: computed,
      // Actions
      fetchData: action,
    });
    // Inject dependencies
    this.routerStore = _rootStore.router;
    this.exampleService = new ExampleService();
  }

  // Computed property (getter)
  get processedData() { /* ... */ }

  // Computed with parameters (computedFn from mobx-utils)
  getDetails = computedFn((id: string) => {
    return this.data?.[id];
  });

  // Async action pattern
  fetchData = async (id: string) => {
    try {
      const response = await this.exampleService.get(id);
      runInAction(() => {
        this.data[id] = response;
      });
      return response;
    } catch (error) {
      runInAction(() => {
        this.error = error;
      });
      throw error;
    }
  };
}
```

**Key patterns:**
- `makeObservable` with explicit field declarations (never `makeAutoObservable`)
- `observable.ref` for scalar values and non-collection objects
- `observable` (default) for Maps, Arrays, and nested objects requiring deep observation
- `computedFn` from `mobx-utils` for parameterized computations (avoids memory leaks)
- `runInAction()` wrapping all state mutations inside async flows
- Optimistic updates: update store immediately, revert on catch
- `set()` from `lodash-es` for immutable nested updates
- Stores receive `CoreRootStore` reference in constructor for cross-store access

**Root store (`apps/web/core/store/root.store.ts`):**
- Central `CoreRootStore` class instantiates all sub-stores
- Each store typed via interface (`IExampleStore`)
- `enableStaticRendering(typeof window === "undefined")` for SSR compatibility
- Cast to `RootStore` (CE/EE) via `this as unknown as RootStore` for enterprise store access

## Import Organization

**Import order convention (observed in code):**
1. React/mobx imports: `import React from "react"`, `import { observer } from "mobx-react"`
2. External libraries: `import { set } from "lodash-es"`
3. Internal packages (with comment headers):
   - `// ui` — `import { Tooltip } from "@plane/propel/tooltip"`
   - `// types` — `import type { IWorkspace } from "@plane/types"`
   - `// constants` — `import { API_BASE_URL } from "@plane/constants"`
   - `// services` — `import { WorkspaceService } from "../api.service"`
   - `// helpers` — `import { cn } from "../utils"`
   - `// hooks` — `import { useWorkspace } from "@/hooks/store/use-workspace"`
   - `// components` — `import { AppHeader } from "@/components/core/app-header"`
   - `// layouts` — `import DefaultLayout from "@/layouts/default-layout"`
   - `// plane web store` — store imports
   - `// local components` — `import { WorkspaceDashboardHeader } from "./header"`

**Comment headers** 是惯用模式——import 用单行注释分组：
```typescript
// ui
import { Button } from "@plane/ui/button";
// types
import type { IUser } from "@plane/types";
// services
import { UserService } from "@/services/user.service";
```

**Path Aliases:**
- `@/*` → `./src/*` (packages) or `./core/*` (apps/web)
- `@plane/ui` → `packages/ui`
- `@plane/types` → `packages/types`
- `@plane/utils` → `packages/utils`
- `@plane/constants` → `packages/constants`
- `@plane/i18n` → `packages/i18n`
- `@plane/services` → `packages/services`
- `@plane/shared-state` → `packages/shared-state`
- `@plane/editor` → `packages/editor`
- `@plane/logger` → `packages/logger`
- `@plane/hooks` → `packages/hooks`
- `@plane/propel` → `packages/propel`
- `@/plane-web/*` → `./ce/*` (community edition / enterprise placeholder)

**Dependency management:**
- Internal packages: `"@plane/types": "workspace:*"`
- External deps: use `catalog:` protocol declared in `pnpm-workspace.yaml`

## API Service Patterns

**Abstract base class (`packages/services/src/api.service.ts`):**
```typescript
export abstract class APIService {
  protected baseURL: string;
  private axiosInstance: AxiosInstance;

  constructor(baseURL: string) {
    this.axiosInstance = axios.create({ baseURL, withCredentials: true });
  }

  get(url, params = {}, config = {}) { return this.axiosInstance.get(url, { ...params, ...config }); }
  post(url, data = {}, config = {}) { return this.axiosInstance.post(url, data, config); }
  put(url, data = {}, config = {}) { return this.axiosInstance.put(url, data, config); }
  patch(url, data = {}, config = {}) { return this.axiosInstance.patch(url, data, config); }
  delete(url, data?, config = {}) { return this.axiosInstance.delete(url, { data, ...config }); }
}
```

**Concrete service (`packages/services/src/workspace/workspace.service.ts`):**
- Extends `APIService` with `super(BASE_URL || API_BASE_URL)`
- Methods return `this.get/post/patch/delete(url, data)`
- Chained: `.then(response => response?.data).catch(error => { throw error?.response?.data; })`
- Returns strongly typed promises: `Promise<IWorkspace[]>`, `Promise<IWorkspace>`
- Constructor accepts optional `BASE_URL` parameter for testing/customization
- Each method has JSDoc with `@param`, `@returns`, `@throws`

**Barrel exports (`packages/services/src/index.ts`):**
- `export * from "./workspace"`, `export * from "./auth"`, etc.
- All services re-exported from a single entry point

## Error Handling Patterns

**Service layer (`.catch()` pattern):**
```typescript
async retrieve(slug: string): Promise<IWorkspace> {
  return this.get(`/api/workspaces/${slug}/`)
    .then((response) => response?.data)
    .catch((error) => {
      throw error?.response?.data;  // or throw error?.response;
    });
}
```

**MobX store layer (try-catch with rollback):**
```typescript
async updateWidget(id: string, data: Partial<TWidget>) {
  const original = { ...this.widgets[id] };
  try {
    runInAction(() => { this.widgets[id] = { ...this.widgets[id], ...data }; });
    const response = await this.service.update(id, data);
    return response;
  } catch (error) {
    runInAction(() => { this.widgets[id] = original; }); // Rollback
    throw error;
  }
}
```

**i18n layer (crash guard):**
- `coerceToString()` in `packages/i18n/src/hooks/use-translation.ts` prevents React crashes when `t()` returns objects instead of strings
- Dev-mode console warning for non-string translation values

**React components:**
- Conditional rendering rather than error boundaries for most cases
- Error states stored in MobX stores (`error: any = null`), components reactively display error UI

## CSS/Styling Conventions

**Framework:** Tailwind CSS 4 (`packages/tailwind-config/`)

**CSS Layers:**
- Custom design tokens defined as CSS custom properties in `packages/tailwind-config/variables.css`
- Variables use OKLCH color space (`--alpha-white-*`, `--alpha-black-*`, `--extended-color-*`)
- Custom variants: `dark`, `dark-high-contrast`, `light-high-contrast` via `data-theme` attribute
- Custom utilities like `@utility conical-gradient`

**Class composition utility:**
- `cn()` from `@plane/utils` combines `clsx` + `tailwind-merge` with custom configuration
- Custom typography classes (`text-h1-semibold`，`text-body-md-regular`) are recognized by `tailwind-merge`
- Custom font size classes (`text-9` through `text-40`)
- Custom text color classes (`text-primary`, `text-on-color`, `text-secondary`, etc.)

**Styling patterns in components:**
```tsx
<div className={cn(
  "grid place-items-center overflow-hidden",
  getBorderRadius(shape),
  { [sizeInfo.avatarSize]: !isAValidNumber(size) }
)}>
```
- Conditional classes use object syntax in `cn()`
- Helper functions generate style variants: `getButtonStyling(variant, size, disabled)` returns string
- Inline styles used sparingly for dynamic values (e.g., computed dimensions)

## i18n Conventions

**Framework:** i18next + react-i18next + i18next-icu (ICU message format)

**Translation file structure:**
```
packages/i18n/src/locales/
  en/
    common.json          # Default namespace
    auth.json
    home.json
    workspace.json
    project.json
    work-item.json
    cycle.json
    module.json
    ... (28 namespaces)
  fr/, es/, ja/, zh-CN/, zh-TW/, ru/, ... (19 languages)
```

**Naming conventions for keys:**
- Nested, snake_case key names: `"issue.label": "Work item"`
- Top-level namespace keys group related translations
- ICU format for dynamic values: `"items": "{count, plural, one {Work item} other {Work items}}"`

**Usage in components:**
```tsx
import { useTranslation } from "@plane/i18n";
const { t } = useTranslation();
// Simple: t("common.submit")
// With params: t("items", { count: 5 })
```

**Hook wrapper (`packages/i18n/src/hooks/use-translation.ts`):**
- Custom `useTranslation()` wraps `react-i18next`'s `useTranslation()` with crash guard
- No namespace argument — `fallbackNS` config searches all namespaces for any key
- Returns: `{ t, currentLocale, changeLanguage, languages }`
- Language persistence in `localStorage` under `LANGUAGE_STORAGE_KEY`

**Adding new translation keys:**
1. Add key to `en/<namespace>.json` (English is base)
2. Add identical key to ALL other language files (can use English as placeholder)
3. Keep nesting structure identical across languages
4. ICU format must be uniform across all languages for the same key

## Git Conventions

**Pre-commit hook (`.husky/pre-commit`):**
- Runs `pnpm lint-staged`
- oxfmt formatting check + fix for all staged files
- oxlint check with auto-fix for TypeScript/JavaScript files

**Issue naming:**
- `🐛 Bug: [description]`
- `🚀 Feature: [description]`
- `🛠️ Improvement: [description]`
- `📘 Docs: [description]`

**Branch:** `preview` is the main development branch
**PR workflow:** CI runs `check:format`, `check:lint`, `check:types` (build required for types)

## Python (Backend) Conventions

**Linting/Formatting (ruff via `apps/api/pyproject.toml`):**
- Line length: 120 characters
- Indent: 4 spaces
- Quote style: double quotes
- Selected rules: `E` (pycodestyle), `F` (Pyflakes)
- McCabe complexity: max 10
- Max args: 8, max statements: 50
- Docstring convention: Google style
- Tests: `E402`, `F401`, `F811` ignored
- `__init__.py`: `F401` ignored
- isort: combine-as-imports, known-first-party: `plane`

**Naming conventions:**
- snake_case for functions, variables, files (`test_workspace_model.py`, `create_user`)
- PascalCase for classes (`TestWorkspaceModel`, `WorkspaceMember`)
- SCREAMING_SNAKE_CASE for constants (`USERS_ME_URL`)

## Documentation Standards

**Copyright headers (EVERY source file):**
```
/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */
```
Appears at the top of every `.ts`, `.tsx`, and `.py` file.

**JSDoc/TSDoc (TypeScript):**
- Service methods have full JSDoc: `@param`, `@returns`, `@throws`, `@remarks`
- Store methods use single-line `@description` format
- Props have inline JSDoc descriptions with `@default` for optional values

**Python docstrings:**
- Google-style docstrings on class methods
- Triple-quote block comments for module/file-level descriptions
- Inline comments for complex logic explanations

## Module Design

**Exports:**
- Named exports preferred over default exports
- Barrel files (`index.ts`) use `export * from "./submodule"` to re-export
- Types and implementations exported from same package (`@plane/types`, `@plane/services`)

**Package structure:**
```
packages/<name>/
  src/
    index.ts      # Barrel re-exports
    <feature>/
      index.ts    # Feature-level barrel
      <file>.ts
  tsconfig.json
  package.json
```

## Function Design

**Size:** Store methods typically 10-30 lines; service methods 5-15 lines; React components 20-100 lines.

**Parameters:** Destructured props in React components. Typed params with explicit types (not inferred). Optional params use `?` modifier.

**Return values:** Async functions return `Promise<T>`. Services return typed data. Stores return `Promise<void>` or `Promise<T>` for fetch actions, `void` for mutations.

---

*Convention analysis: 2026-06-16*
