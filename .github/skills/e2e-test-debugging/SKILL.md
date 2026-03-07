---
name: e2e-test-debugging
description: Systematic methodology for running, diagnosing, and fixing Playwright E2E test failures in this project. Use this when running E2E tests, investigating flaky or failing E2E tests, verifying infrastructure readiness, or debugging issues where UI elements behave unexpectedly (buttons stay disabled, pages redirect to error, config values are wrong). Also use this when the user mentions "e2e", "playwright", "end-to-end", or asks to verify that the app works.
---

# E2E Test Debugging

Systematic methodology for running, diagnosing, and fixing Playwright E2E test failures.

## Project E2E Architecture

The E2E suite uses Playwright across three browsers (Chromium, Firefox, WebKit) with parallel workers. The test infrastructure involves:

- **Vite dev server** (`localhost:5173`) — serves the Vue SPA and proxies `/api`, `/config.js`, `/analytics.js` to the backend
- **ASP.NET Core backend** (`https://localhost:8001`) — the API server, started with the `e2e-local` launch profile
- **Docker services** — KeyDB (Redis), HashiCorp Vault, S3 mock (Adobe S3Mock)
- **Config pipeline** — backend `appsettings.json` → options binding → `/config.js` endpoint → `window.appConfig` → Vue composables/components

### Key Files

| File | Purpose |
|---|---|
| `Warp.ClientApp/playwright.config.ts` | Test config: timeouts, browsers, web server |
| `Warp.ClientApp/e2e/global-setup.ts` | Verifies dev server and backend health before tests |
| `Warp.ClientApp/e2e/locators.ts` | Centralized Playwright locator definitions |
| `Warp.ClientApp/e2e/utils.ts` | Shared helpers: `clickElement`, `fillTextAndVerify`, `saveAndAwaitPostSaveButtons` |
| `Warp.ClientApp/e2e/*.spec.ts` | Test specs |
| `Warp.WebApp/Properties/launchSettings.json` | Launch profiles including `e2e-local` |
| `Warp.WebApp/appsettings.E2ELocal.json` | Backend config for local E2E (localhost addresses) |
| `docker-compose.e2e.yml` | Vault, vault-init, S3 mock services |
| `docker-compose.yml` | KeyDB service |

## Pre-Flight: Infrastructure Verification

Before investigating test failures, confirm the entire stack is operational. Many failures that look like test bugs are actually infrastructure problems.

### Step 1: Docker Services

```bash
docker compose -f docker-compose.yml -f docker-compose.e2e.yml ps
```

Verify **keydb**, **vault**, and **s3mock** are all `Up`. If any are down:

```bash
docker compose -f docker-compose.yml -f docker-compose.e2e.yml up -d
```

### Step 2: Vault Token

The backend needs a valid Vault token at `.vault/warp-e2e.token`. The `vault-init` Docker service generates this automatically. If missing, restart the vault services.

### Step 3: Backend Server

The .NET backend must be running on `https://localhost:8001` with the `e2e-local` profile:

```bash
dotnet run --project Warp.WebApp --launch-profile e2e-local
```

If the backend isn't running, the Vite proxy returns `ECONNREFUSED` for all API calls and the global setup will time out with "Vue app did not initialize within 120000ms" after showing repeated proxy errors.

### Step 4: Playwright Browsers

If Playwright was recently updated, browsers may need reinstalling:

```bash
cd Warp.ClientApp && yarn playwright install
```

Error signature: `browserType.launch: Executable doesn't exist at ...chrome-headless-shell.exe`

### Running the Tests

From `Warp.ClientApp/`:

```bash
yarn e2e              # all browsers, headless
yarn e2e:headed       # Chromium only, headed (for visual debugging)
```

## Diagnostic Workflow

### 1. Classify the Failure Pattern

Before reading individual error details, look at the **full test summary** to classify:

| Pattern | Likely cause | Where to look |
|---|---|---|
| Same failure across **all 3 browsers** | Code bug or config issue | Application code, config pipeline |
| Failure in **one browser only** | Timing/flakiness | Timeouts, parallel worker contention |
| **All tests** fail identically | Infrastructure or config | Pre-flight checks, `/config.js` output |
| Specific **test group** fails | Feature-specific bug | The feature's component and API |

This classification saves significant time — a config binding mismatch causing all UI validation to fail looks very different from a single flaky timeout.

### 2. Collect Error Context

Read the **error message**, **call stack**, and **error-context artifacts** produced by Playwright:

- Error message reveals which locator timed out and with what timeout.
- Call stack pinpoints the exact spec line and utility function involved.
- `error-context.md` (or equivalent snapshot in `test-results/`) captures the **actual DOM/accessibility tree at failure time** — this is the single most valuable artifact.

### 3. Read the Failing Test and Its Helpers

Read the full test case, its locator definitions in `e2e/locators.ts`, and utility functions in `e2e/utils.ts`. Understand:

- The exact sequence of user actions the test performs.
- Which locators are used and how they resolve (role, text, test-id).
- Default timeouts baked into helper functions (e.g., `clickElement` uses 15s, `saveAndAwaitPostSaveButtons` uses 30s).

### 4. Analyze the Page Snapshot

Compare the **expected UI state** (what the test waits for) against the **actual UI state** (from the error-context snapshot):

| Symptom in snapshot | Likely cause |
|---|---|
| "saving..." / spinner visible, button shows "Pending…" | Async operation (API call) hasn't completed; timeout too short or request failed silently |
| Error page or redirect to `/error` | API returned an error status or ECONNREFUSED; check backend is running |
| Element missing entirely | Wrong route, component not mounted, or conditional rendering (`v-if`) not satisfied |
| Element present but **disabled** | State flag not updated, validation failing, or config values wrong |
| All pages show `/error?status=500` | Backend not reachable; check Vite proxy and backend process |

### 5. Trace the Config Pipeline

When elements are stuck in wrong states (e.g., button permanently disabled despite valid input), trace the config value flow:

1. **Backend config**: Check the relevant section in `appsettings.json` / `appsettings.E2ELocal.json` (e.g., `EntryValidatorOptions`)
2. **Options binding**: Check `ServiceCollectionExtensions.cs` for `.BindConfiguration(nameof(...))` — the section name must match the JSON key
3. **`/config.js` endpoint**: Check `SpaExtensions.cs` `MapSpaConfigs` — verify it reads from the correct config path or options object
4. **Frontend consumption**: Check `window.appConfig` usage in composables (e.g., `use-content-size-indicator.ts`) and how components derive their state from these values

A mismatch at any step (e.g., `/config.js` reading `Content:SizeLimits:MaxPlainTextSize` when the actual config section is `EntryValidatorOptions:MaxPlainTextSize`) causes the frontend to receive default/zero values, breaking all validation-dependent UI.

### 6. Cross-Reference with Application Code

Trace the flow through the application:

- **Component template**: Check `:disabled` bindings and `v-if`/`v-else` conditions that gate the target element's visibility or state.
- **Component script**: Follow the async handler (e.g., `onSave`) — identify all awaited calls and what sets the flag the template depends on.
- **API layer**: Check the request construction (FormData, headers, auth) and error handling (`try/catch/finally`).
- **Server-side**: Verify the endpoint exists, check attribute filters (auth cookies, content-type validation), and confirm the request format matches.

### 7. Compare with Similar Passing Tests

Find other tests that perform the same flow (e.g., save → copy link) and compare:

- Do passing tests use explicit waits (`await expect(...).toBeVisible({ timeout })`) that the failing test lacks?
- Do passing tests skip expensive operations (e.g., image uploads) that add latency?

## Common Fix Patterns

### Configuration Binding Mismatch

**Problem**: The `/config.js` endpoint reads config values from a path that doesn't match the actual appsettings section. The frontend receives `0` or `null` for limits/settings, causing validation to reject all input. Symptom: elements (e.g., Preview button) stay disabled across all browsers, even after entering valid content.

**Diagnosis**: Check if the config path in `SpaExtensions.cs` matches the section name in `appsettings.json`. Use strongly-typed `IOptions<T>` instead of raw `IConfiguration.GetValue<>()` calls with string paths — this catches mismatches at startup.

**Fix**: Inject the options object and read from it:

```csharp
// Before (broken — wrong path)
["maxPlainTextContentSize"] = configuration.GetValue<int>("Content:SizeLimits:MaxPlainTextSize")

// After (correct — uses bound options)
["maxPlainTextContentSize"] = entryValidatorOptions.Value.MaxPlainTextSize
```

### Insufficient Timeout After Expensive Operations

**Problem**: Default `clickElement` timeout (15s) is too short for operations involving file uploads, image processing, or heavy server load.

**Fix**: Use `saveAndAwaitPostSaveButtons` or create a dedicated helper that waits for the post-operation UI state with a generous timeout.

**Key principle**: Separate the *action* (click Save) from the *assertion* (wait for result) so the wait timeout can be tuned independently.

### Missing Wait Between Sequential Actions

**Problem**: Two `clickElement` calls back-to-back where the second element only appears after the first action's async side-effect completes.

**Fix**: Insert an explicit visibility/state assertion between the two clicks:

```ts
await clickElement(getSaveButton(page))
await expect(getCopyLinkButton(page)).toBeVisible({ timeout: 30000 })
await clickElement(getCopyLinkButton(page))
```

### Silent API Failures

**Problem**: The API call fails (network error, 401, 500) but the error is caught and only logged to console. The UI stays stuck in "saving" state forever.

**Diagnosis**: Check the error-context snapshot for redirect to error pages or stuck spinner states. Add `page.on('console')` or `page.on('response')` listeners in the test to capture API failures.

### Global Setup Timeout

**Problem**: `global-setup.ts` times out with "Vue app did not initialize within 120000ms". Logs show repeated `ECONNREFUSED` errors from the Vite proxy.

**Fix**: This always means the backend is unreachable. Verify that `dotnet run --project Warp.WebApp --launch-profile e2e-local` is running and healthy on `https://localhost:8001`.

## Checklist

- [ ] Verify infrastructure: Docker services up, Vault token exists, backend running, Playwright browsers installed
- [ ] Classify failure pattern: all-browser (code/config) vs single-browser (timing)
- [ ] Read error message, call stack, and error-context snapshot
- [ ] Read the full test case and all referenced helpers/locators
- [ ] Analyze the page snapshot: what is the actual UI state vs expected?
- [ ] If elements stuck disabled: trace the config pipeline (appsettings → options binding → `/config.js` → `window.appConfig` → component)
- [ ] Trace the application flow: template conditions → async handler → API call → server endpoint
- [ ] Compare with similar passing tests for missing waits
- [ ] Apply fix, restart backend if server-side change, and verify across all browsers
