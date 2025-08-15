# Vue.js Integration Roadmap (SpaProxy + Sentry)

Scope
- Add a Vue 3 + Vite SPA under /app alongside existing Razor Pages (.NET 9).
- Single-F5 in Visual Studio 2022 via SpaProxy.
- Sentry enabled end-to-end (backend + Vue SPA), with source maps and environment tagging.

Outcomes
- F5 starts ASP.NET Core and Vite dev server automatically.
- Navigating to /app loads the SPA; client routes work on refresh.
- Publish includes built SPA in wwwroot/app with correct cache headers and fallback.
- Sentry captures frontend and backend issues with proper environment and release metadata.

Phase 1 � Decisions (locked) - Done
- Dev model: SpaProxy (one F5; no CORS needed in dev).
- Package manager: Yarn (Berry), node-modules linker to keep VS tooling simple.
- SPA mount path: /app (prevents route conflicts with Razor Pages).
- Language/tooling: Vue 3 + TypeScript + Vite.
- Observability: Sentry for backend and Vue SPA (performance + optional session replay).

Phase 2 � Project layout - Done
- ClientApp folder created at the solution root with a README placeholder.
- Keep existing wwwroot/js and current Vite build for Razor Pages features as-is.
- Add ClientApp to the solution for visibility in Solution Explorer (via Solution Explorer > Add > Existing Folder when using Visual Studio).

Phase 3 � Initialize the Vue app - Done
- Folder renamed to Warp.ClientApp for consistency.
- Vue 3 + Vite scaffolded in Warp.ClientApp (TypeScript enabled).
- Yarn configured (Berry) with node-modules linker; scripts: dev/build/preview.
- Vite base set to /app; dev server port 5173; build outputs to dist with sourcemaps.
- Router configured with createWebHistory('/app/').
- Minimal views added (Home, About) to validate routing and refresh.

Phase 4 � Developer experience (SpaProxy) - Done
- Add SpaProxy to the web project and configure it to:
  - Launch yarn dev in Warp.ClientApp on F5.
  - Proxy all /app/* requests to http://localhost:5173 in Development.
- In Production, ASP.NET Core serves static files from wwwroot/app and falls back /app/* to the SPA�s index.
- Visual Studio:
  - Use Debug Target for Kestrel or IIS Express.
  - F5 launches both servers; no separate terminal needed.
  - If proxy startup is slow, adjust SpaProxy timeout accordingly.

Phase 5 � Backend integration - Done
- Keep existing Razor Pages endpoints and middleware order.
- Serve static files and add a fallback for client-side routing under /app.
- Expose APIs under /api for SPA consumption; follow consistent JSON and error shape.

Phase 6 � Auth and security - Done
- Prefer same-origin cookie auth in Production; no CORS required.
- Define CSRF strategy for state-changing endpoints (anti-forgery tokens or SameSite cookies).
- In Dev with SpaProxy, avoid broad CORS; SpaProxy keeps requests same-origin via the backend proxy.

Phase 7 � Styling - Done
- Decide whether the SPA uses Tailwind/Sass independently or via a shared package.
- Align tokens (colors, spacing, typography) to maintain a single design language.

Phase 8 � Frontend quality gates
- Enable ESLint, Prettier, and TypeScript strict mode with rules aligned to the repo.
- Add commit hooks (lint-staged) and enforce in CI.

Phase 9 � Testing
- Unit: Vitest + Vue Test Utils.
- E2E: Playwright or Cypress targeting /app routes and deep-link refresh.
- Add smoke tests for critical API interactions.

Phase 10 � Build and publish
- Production build sequence:
  - yarn install (immutable) in Warp.ClientApp.
  - yarn build to produce dist.
  - Copy dist to wwwroot/app in the web project.
- Automate on Publish (MSBuild target) so VS publishing and CI/CD match.
- Cache headers:
  - Long cache for hashed assets; short cache for index.html.
  - Enable compression on the server.

Phase 11 � Observability (Sentry)
- Backend: continue using Sentry.AspNetCore with environment tags.
- Vue SPA:
  - Use Sentry�s Vue SDK with router tracing; enable performance monitoring and optional session replay.
  - Initialize with DSN and environment from a shared runtime config (appConfig) so values aren�t hardcoded (aligns with existing wwwroot/js/services/sentry.js pattern).
  - Skip Sentry in Local environment to match existing behavior.
  - Ensure errors are grouped with a meaningful release version (e.g., Git SHA) and upload source maps in CI.
  - Correlate with backend using W3C trace context (traceparent) or a request ID header; include it in client error context when available.
  - Scrub PII prior to capture according to policy.

Phase 12 � CI/CD and Docker
- CI:
  - Cache Yarn and Vite artifacts.
  - Build .NET and Warp.ClientApp; run tests; upload Sentry source maps with the same release version.
  - Publish artifacts with SPA under wwwroot/app.
- Docker:
  - Multi-stage build (Node to build Warp.ClientApp, ASP.NET runtime to serve).
  - Health checks validate API and SPA availability.

Phase 13 � Documentation and onboarding
- Document prerequisites (Node, Corepack, Yarn), F5 flow with SpaProxy, and how to run unit/E2E tests.
- Add troubleshooting (proxy timeouts, port conflicts, DSN/env config).

Phase 14 � Incremental rollout and acceptance criteria
- Milestone 1: Scaffolding + SpaProxy wired + /app loads Hello World.
- Milestone 2: API calls from SPA to /api; auth and CSRF finalized; base styling.
- Milestone 3: Linting, testing, TypeScript strict; CI green.
- Milestone 4: Publish automation + Docker; Sentry source maps and release versioning; docs complete.

Acceptance
- One F5 in Visual Studio starts both servers; /app works with client routing.
- Production publish serves built assets from wwwroot/app with proper caching and fallback.
- Sentry captures frontend and backend errors with environment and release tags; source maps resolve stack traces.
