# ClientApp

This folder hosts the Vue 3 + Vite Single Page Application mounted under /app.

Notes
- Created as part of Phase 2 (Project layout) of the Vue integration roadmap.
- Existing Razor Pages assets and Vite build for wwwroot/js remain unchanged.
- In Phase 3 we will scaffold the Vue app here and configure Yarn and SpaProxy.

## Internationalization (i18n)

The app uses `vue-i18n` (composition API, lazy‑loaded TS locale modules) located under `src/i18n`.

Key points:
- Supported locales: `en`, `es` (extend `supportedLocales` in `detect.ts`).
- Detection order: persisted (`localStorage` key `warp.locale`) → exact navigator match → primary subtag → fallback `en`.
- Locale messages are lazy loaded on first use to avoid inflating the initial bundle.
- `createI18nInstance()` is awaited in `main.ts` before mounting; switching uses `setLocale(locale)`.
- The current `<html lang>` attribute is kept in sync for accessibility and SEO.

# ClientApp

This folder contains the frontend Single Page Application for Warp — a Vite + Vue 3 + TypeScript app mounted under `/app` in the server project.

## Overview

- Framework: Vue 3 (Composition API)
- Bundler / dev server: Vite
- Language: TypeScript
- Styling: Tailwind CSS + PostCSS
- Unit tests: Vitest + @testing-library/vue
- E2E tests: Playwright (config present under `playwright.config.ts`)
- Linting & formatting: ESLint, Prettier, Husky + lint-staged for pre-commit checks

Files worth checking:
- `src/` — app source
- `src/i18n` — internationalization implementation (lazy-loaded locale modules)
- `src/__tests__` — unit tests
- `e2e/` — Playwright end-to-end tests
- `vite.config.ts`, `vitest.config.ts`, `playwright.config.ts` — tooling configs

## Quick start (developer)

Prerequisites: Node.js (recommended 18+), Yarn (this repo uses Yarn v1.x but npm works for many flows).

Install deps:

```bash
yarn install
```

Run dev server (hot reload):

```bash
yarn dev
```

Build for production:

```bash
yarn build
```

Preview the production build locally (server):

```bash
yarn preview
```

Type-check the project:

```bash
yarn typecheck
```

## Scripts

- `dev` — start Vite dev server
- `build` — build production assets
- `preview` — serve built assets locally
- `typecheck` — run `vue-tsc` (type-only check)
- `lint` / `format` — ESLint & Prettier
- `test` — run Vitest once
- `test:watch` — run Vitest in watch mode
- `e2e` — run Playwright against the Vite dev server (auto-started)
- `e2e:headed` — run the Playwright suite in headed Chromium for debugging
- `e2e:install` — download/refresh the Playwright browser binaries

These scripts are defined in `package.json` in this folder.

## Testing

Unit tests
- Vitest is used for unit testing and DOM tests via `@testing-library/vue`.

Run unit tests:

```bash
yarn test
```

Watch mode:

```powershell
yarn test:watch
```

End-to-end tests
- A Playwright configuration is included (`playwright.config.ts`) and quick e2e tests live under `e2e/`.
- The suite now runs via package scripts; the config auto-starts the Vite dev server (`yarn dev --host 127.0.0.1 --port 5173`).
- Override defaults with `E2E_DEV_SERVER_PORT` or `E2E_DEV_SERVER_COMMAND` (e.g., point to a pre-built server or tunnel).

Run the Playwright suite (from `Warp.ClientApp`):

```powershell
yarn e2e
```

Headed Chromium run:

```powershell
yarn e2e:headed
```

Install/refresh browser binaries (first-time setup or CI cache misses):

```powershell
yarn e2e:install
```

From the repo root you can also run:

```powershell
yarn e2e:client
```

The root command mirrors other client scripts and ensures dependencies are installed before kicking off the E2E run.

## Internationalization (i18n)

The app uses `vue-i18n` with a lazy-loaded, composition-API friendly setup. Key points:

- Locale files live under `src/i18n/locales` and are dynamically imported on first use to keep the initial bundle small.
- Detection order: persisted (`localStorage` key `warp.locale`) → exact navigator match → primary subtag → fallback `en`.
- Use `useI18n()` inside components to access translations.

Runtime example (programmatic):

```ts
import { setLocale } from '@/i18n'
await setLocale('es')
```

To add a new locale: add a locale module under `src/i18n/locales`, register the code in the detection list, and add the dynamic import case in the loader.

## Integration with ASP.NET backend

- Built assets are consumed by the ASP.NET Core project and are mounted under `/app` in the server.
- `spa.proxy.json` is provided for dev-time proxying when running the .NET backend together with the Vite dev server.

## Conventions & tooling

- Keep TypeScript types strict and run `yarn typecheck` before pushing.
- Husky + lint-staged runs Prettier and ESLint on staged files to enforce style.
- Prefer using named parameters and following the repository's code style when editing frontend code.

## Where to look next

- App entry: `src/main.ts`
- Routing: `src/router`
- Pages & views: `src/views` and `src/pages`
- Shared components: `src/components`

## Short checklist for common tasks

- Start dev server: `yarn dev`
- Run unit tests: `yarn test`
- Run e2e tests: `npx playwright test`
- Build production: `yarn build`

---

If you'd like, I can also add a short `DEVELOPING.md` with the SPA → ASP.NET integration notes and example proxy setup.

