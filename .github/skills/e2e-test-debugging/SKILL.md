---
name: e2e-test-debugging
description: Systematic methodology for diagnosing and fixing Playwright E2E test failures. Use this when investigating flaky or failing E2E tests.
---

# E2E Test Debugging

Systematic methodology for diagnosing and fixing Playwright E2E test failures.

## Diagnostic Workflow

### 1. Collect Error Context

Read the **error message**, **call stack**, and **error-context artifacts** (page snapshots, screenshots) produced by Playwright:

- Error message reveals which locator timed out and with what timeout.
- Call stack pinpoints the exact spec line and utility function involved.
- `error-context.md` (or equivalent snapshot) captures the **actual DOM/accessibility tree at failure time** — this is the single most valuable artifact.

### 2. Read the Failing Test and Its Helpers

Read the full test case, its locator definitions, and utility functions (e.g., `clickElement`, `fillTextAndVerify`). Understand:

- The exact sequence of user actions the test performs.
- Which locators are used and how they resolve (role, text, test-id).
- Default timeouts baked into helper functions.

### 3. Analyze the Page Snapshot

Compare the **expected UI state** (what the test waits for) against the **actual UI state** (from the error-context snapshot):

| Symptom in snapshot | Likely cause |
|---|---|
| "saving..." / spinner visible, button shows "Pending…" | Async operation (API call) hasn't completed; timeout too short or request failed silently |
| Error page or redirect | API returned an error status; check server logs, auth cookies, request filters |
| Element missing entirely | Wrong route, component not mounted, or conditional rendering (`v-if`) not satisfied |
| Element present but disabled | State flag not updated, or prerequisite action incomplete |

### 4. Cross-Reference with Application Code

Trace the flow through the application:

- **Component template**: Check `v-if`/`v-else` conditions that gate the target element's visibility (e.g., `saved` flag toggling between Save and Copy Link buttons).
- **Component script**: Follow the async handler (e.g., `onSave`) — identify all awaited calls and what sets the flag the template depends on.
- **API layer**: Check the request construction (FormData, headers, auth) and error handling (`try/catch/finally`).
- **Server-side**: Verify the endpoint exists, check attribute filters (auth cookies, content-type validation), and confirm the request format matches.

### 5. Check Cross-Browser and Parallel Results

Review the **full test output** for the same test across browsers:

- If the test **passes on some browsers but fails on others**, it's likely a **timing/flakiness issue**, not a code bug.
- If it **fails consistently everywhere**, look for a logic error or missing prerequisite.
- Note the **execution times** — a test that takes 37s on Firefox but times out at 15s on Chromium points to insufficient timeouts under load.
- Check how many **parallel workers** ran — more workers means more backend pressure and slower API responses.

### 6. Compare with Similar Passing Tests

Find other tests that perform the same flow (e.g., save → copy link) and compare:

- Do passing tests use explicit waits (`await expect(...).toBeVisible({ timeout })`) that the failing test lacks?
- Do passing tests skip expensive operations (e.g., image uploads) that add latency?

## Common Fix Patterns

### Insufficient Timeout After Expensive Operations

**Problem**: Default `clickElement` timeout (e.g., 15s) is too short for operations involving file uploads, image processing, or heavy server load.

**Fix**: Create a dedicated helper that waits for the post-operation UI state with a generous timeout:

```ts
export async function saveAndAwaitPostSaveButtons(page: Page, timeout = 30000): Promise<void> {
    const saveButton = page.getByRole('button', { name: /^Save$/i })
    await clickElement(saveButton)

    const copyLinkButton = page.getByRole('button', { name: /copy link/i })
    await expect(copyLinkButton).toBeVisible({ timeout })
}
```

**Key principle**: Separate the *action* (click Save) from the *assertion* (wait for result) so the wait timeout can be tuned independently.

### Missing Wait Between Sequential Actions

**Problem**: Two `clickElement` calls back-to-back where the second element only appears after the first action's async side-effect completes.

**Fix**: Insert an explicit visibility/state assertion between the two clicks:

```ts
await clickElement(getSaveButton(page))
await expect(getCopyLinkButton(page)).toBeVisible({ timeout: 30000 })  // wait for save to complete
await clickElement(getCopyLinkButton(page))
```

### Silent API Failures

**Problem**: The API call fails (network error, 401, 500) but the error is caught and only logged to console. The UI stays stuck in "saving" state forever.

**Diagnosis**: Check the error-context snapshot for redirect to error pages or stuck spinner states. Add `page.on('console')` or `page.on('response')` listeners in the test to capture API failures.

## Checklist

- [ ] Read error message, call stack, and error-context snapshot
- [ ] Read the full test case and all referenced helpers/locators
- [ ] Analyze the page snapshot: what is the actual UI state vs expected?
- [ ] Trace the application flow: template conditions → async handler → API call → server endpoint
- [ ] Check cross-browser results: flaky (timing) vs consistent (logic) failure
- [ ] Compare with similar passing tests for missing waits
- [ ] Apply fix and verify across all browsers
